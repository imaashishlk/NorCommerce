using Bogus;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NorCommerce.DataAccess.Data;
using NorCommerce.Models;
using NorCommerce.Utility;
using System.Text;
using System.Text.Json;

namespace NorCommerce.DataAccess.Seed
{
    /// <summary>
    /// Fills the database with realistic demo data. Run in Development only.
    /// Safe to run on every start: each step checks if its data is already there.
    ///
    ///   1. 8 categories + 80 books (covers looked up on Open Library)
    ///   2. 1 admin user
    ///   3. 200 Norwegian customers (Bogus, nb_NO)
    ///   4. ~3 years of orders with growth, seasons and bestsellers
    /// </summary>
    public static class DevDataSeeder
    {
        private const int CustomerCount = 200;
        private const int YearsOfSales = 3;
        private const string CustomerEmailDomain = "@example.no";
        private const string CustomerPassword = "Test123!";

        private const string AdminEmail = "admin@norcommerce.no";
        private const string AdminPassword = "Admin123!";

        // Real postal code + city + county pairs. Rogaland is listed more often on purpose.
        private static readonly (string PostalCode, string City, string County)[] Places =
        {
            ("4006", "Stavanger", "Rogaland"),
            ("4014", "Stavanger", "Rogaland"),
            ("4021", "Stavanger", "Rogaland"),
            ("4306", "Sandnes", "Rogaland"),
            ("4050", "Sola", "Rogaland"),
            ("5525", "Haugesund", "Rogaland"),
            ("0150", "Oslo", "Oslo"),
            ("0250", "Oslo", "Oslo"),
            ("0557", "Oslo", "Oslo"),
            ("5003", "Bergen", "Vestland"),
            ("5055", "Bergen", "Vestland"),
            ("7011", "Trondheim", "Trøndelag"),
            ("4608", "Kristiansand", "Agder"),
            ("9008", "Tromsø", "Troms"),
            ("3015", "Drammen", "Buskerud"),
            ("1607", "Fredrikstad", "Østfold"),
            ("6002", "Ålesund", "Møre og Romsdal"),
            ("8006", "Bodø", "Nordland"),
            ("2317", "Hamar", "Innlandet"),
            ("3110", "Tønsberg", "Vestfold"),
        };

        private static readonly string[] Carriers = { "Posten", "Bring", "PostNord", "Helthjem" };

        // Seasonal shopping pattern, Jan..Dec. Christmas is the big peak, July is quiet.
        private static readonly double[] MonthFactor =
            { 0.70, 0.80, 0.90, 1.00, 1.00, 0.85, 0.60, 0.90, 1.00, 1.10, 1.50, 1.90 };

        public static async Task SeedAsync(ApplicationDbContext db, UserManager<ApplicationUser> userManager, ILogger logger)
        {
            var random = new Random(2026); // fixed seed: same data every reseed

            await SeedCatalogAsync(db, random, logger);
            await SeedAdminAsync(userManager, logger);
            var customers = await SeedCustomersAsync(db, userManager, logger);
            await SeedOrdersAsync(db, customers, random, logger);
        }

        // ------------------------------------------------------------------
        // 1. Categories + books
        // ------------------------------------------------------------------
        private static async Task SeedCatalogAsync(ApplicationDbContext db, Random random, ILogger logger)
        {
            if (await db.Products.CountAsync() >= 50) return;

            // Categories (reuse existing ones with the same name)
            var categories = await db.Categories.ToListAsync();
            int displayOrder = categories.Count == 0 ? 1 : categories.Max(c => c.DisplayOrder) + 1;

            foreach (var name in BookCatalog.Categories)
            {
                if (categories.All(c => c.Name != name))
                {
                    var category = new Category { Name = name, DisplayOrder = displayOrder++ };
                    db.Categories.Add(category);
                    categories.Add(category);
                }
            }
            await db.SaveChangesAsync();

            var existingTitles = await db.Products.Select(p => p.Title).ToListAsync();

            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            http.DefaultRequestHeaders.UserAgent.ParseAdd("NorCommerceDemo/1.0 (portfolio project)");

            int added = 0;
            foreach (var book in BookCatalog.Books)
            {
                if (existingTitles.Contains(book.Title)) continue;

                var (coverUrl, isbn) = await LookupOpenLibraryAsync(http, book);

                double listPrice = EndIn9(random.Next(229, 460));

                db.Products.Add(new Product
                {
                    Title = book.Title,
                    Author = book.Author,
                    Description = book.Description,
                    ISBN = isbn ?? "N/A",
                    ListPrice = listPrice,
                    Price = EndIn9(listPrice * 0.85),
                    Price50 = EndIn9(listPrice * 0.78),
                    Price100 = EndIn9(listPrice * 0.72),
                    CategoryId = categories.First(c => c.Name == book.Category).Id,
                    ImageUrl = coverUrl ?? string.Empty   // column is NOT NULL, views treat "" as "no image"
                });

                added++;
                await Task.Delay(250); // be polite to the Open Library API
            }

            await db.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} books", added);
        }

        /// <summary>
        /// Asks Open Library for a cover id and an ISBN. Returns nulls if nothing is found
        /// or the request fails, so the seeder never crashes because of the network.
        /// </summary>
        private static async Task<(string? CoverUrl, string? Isbn)> LookupOpenLibraryAsync(HttpClient http, SeedBook book)
        {
            try
            {
                var title = Uri.EscapeDataString(book.SearchTitle ?? book.Title);
                var url = $"https://openlibrary.org/search.json?title={title}&limit=1&fields=cover_i,isbn";

                var author = book.SearchAuthor ?? (book.Author == "Ukjent" ? null : book.Author);
                if (author != null) url += $"&author={Uri.EscapeDataString(author)}";

                using var doc = JsonDocument.Parse(await http.GetStringAsync(url));
                if (!doc.RootElement.TryGetProperty("docs", out var docs) || docs.GetArrayLength() == 0)
                    return (null, null);

                var first = docs[0];

                string? coverUrl = first.TryGetProperty("cover_i", out var cover)
                    ? $"https://covers.openlibrary.org/b/id/{cover.GetInt64()}-L.jpg"
                    : null;

                string? isbn = null;
                if (first.TryGetProperty("isbn", out var isbns))
                {
                    var all = isbns.EnumerateArray().Select(x => x.GetString()).Where(x => x != null).ToList();
                    isbn = all.FirstOrDefault(x => x!.Length == 13) ?? all.FirstOrDefault();
                }

                return (coverUrl, isbn);
            }
            catch
            {
                return (null, null);
            }
        }

        // Shop-style prices: 299, 349, 189...
        private static double EndIn9(double value) => Math.Max(49, Math.Round(value / 10) * 10 - 1);

        // ------------------------------------------------------------------
        // 2. Admin
        // ------------------------------------------------------------------
        private static async Task SeedAdminAsync(UserManager<ApplicationUser> userManager, ILogger logger)
        {
            if (await userManager.FindByEmailAsync(AdminEmail) != null) return;

            var admin = new ApplicationUser
            {
                UserName = AdminEmail,
                Email = AdminEmail,
                EmailConfirmed = true,
                Name = "NorCommerce Admin",
                PhoneNumber = "+47 51 00 00 00",
                StreetAddress = "Kirkegata 1",
                City = "Stavanger",
                State = "Rogaland",
                PostalCode = "4006"
            };

            var result = await userManager.CreateAsync(admin, AdminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, StaticDetails.RoleAdmin);
                logger.LogInformation("Seeded admin user {Email}", AdminEmail);
            }
        }

        // ------------------------------------------------------------------
        // 3. Customers
        // ------------------------------------------------------------------
        private static async Task<List<ApplicationUser>> SeedCustomersAsync(
            ApplicationDbContext db, UserManager<ApplicationUser> userManager, ILogger logger)
        {
            var customers = await db.ApplicationUsers
                .Where(u => u.Email!.EndsWith(CustomerEmailDomain))
                .ToListAsync();

            if (customers.Count >= CustomerCount) return customers;

            Randomizer.Seed = new Random(2026);
            var faker = new Faker("nb_NO");

            // Same password for everyone, so hash it once (hashing is slow on purpose).
            var passwordHash = userManager.PasswordHasher.HashPassword(new ApplicationUser(), CustomerPassword);

            for (int i = customers.Count; i < CustomerCount; i++)
            {
                var firstName = faker.Name.FirstName();
                var lastName = faker.Name.LastName();
                var place = faker.PickRandom(Places);
                var email = $"{ToEmailPart(firstName)}.{ToEmailPart(lastName)}{i + 1}{CustomerEmailDomain}";

                var user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    Name = $"{firstName} {lastName}",
                    PhoneNumber = "+47 " + faker.Random.ReplaceNumbers(faker.PickRandom("4## ## ###", "9## ## ###")),
                    StreetAddress = faker.Address.StreetAddress(),
                    City = place.City,
                    State = place.County,
                    PostalCode = place.PostalCode,
                    PasswordHash = passwordHash
                };

                var result = await userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, StaticDetails.RoleCustomer);
                    customers.Add(user);
                }
            }

            logger.LogInformation("Customers in database: {Count}", customers.Count);
            return customers;
        }

        // "Øyvind" -> "oyvind", "Ås" -> "as"
        private static string ToEmailPart(string value)
        {
            var sb = new StringBuilder();
            foreach (var ch in value.ToLowerInvariant())
            {
                sb.Append(ch switch
                {
                    'æ' => "ae",
                    'ø' => "o",
                    'å' => "a",
                    'é' or 'è' => "e",
                    _ when char.IsLetter(ch) && ch < 128 => ch.ToString(),
                    _ => ""
                });
            }
            return sb.ToString();
        }

        // ------------------------------------------------------------------
        // 4. Orders
        // ------------------------------------------------------------------
        private static async Task SeedOrdersAsync(
            ApplicationDbContext db, List<ApplicationUser> customers, Random random, ILogger logger)
        {
            if (customers.Count == 0) return;
            if (await db.OrderHeaders.CountAsync() >= 500) return;

            var products = await db.Products.ToListAsync();
            if (products.Count == 0) return;

            // Bestsellers sell a lot more, the rest vary a bit
            var bestsellers = BookCatalog.Books.Where(b => b.Bestseller).Select(b => b.Title).ToHashSet();
            var productWeights = products
                .Select(p => bestsellers.Contains(p.Title) ? 6.0 : 1.0 + random.NextDouble() * 2)
                .ToArray();

            // About 20% of customers are loyal and buy much more often
            var customerWeights = customers
                .Select(_ => random.NextDouble() < 0.2 ? 6.0 : 1.0)
                .ToArray();

            var today = DateTime.UtcNow.Date;
            var start = today.AddYears(-YearsOfSales);
            int totalDays = (today - start).Days;

            db.ChangeTracker.AutoDetectChangesEnabled = false;

            var batch = new List<OrderHeader>();
            int total = 0;

            for (int day = 0; day <= totalDays; day++)
            {
                var date = start.AddDays(day);

                // Store grows from ~0.8 to ~3 orders a day, plus seasons and busier weekends
                double growth = 0.8 + 2.2 * day / totalDays;
                double weekend = date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday ? 1.2 : 1.0;
                double expected = growth * MonthFactor[date.Month - 1] * weekend;

                int ordersToday = Poisson(random, expected);

                for (int i = 0; i < ordersToday; i++)
                {
                    var customer = customers[WeightedIndex(random, customerWeights)];
                    var orderDate = date
                        .AddHours(PickHour(random))
                        .AddMinutes(random.Next(60));

                    batch.Add(BuildOrder(random, customer, products, productWeights, orderDate, today));
                }

                if (batch.Count >= 300)
                {
                    total += await SaveBatchAsync(db, batch);
                }
            }

            total += await SaveBatchAsync(db, batch);
            db.ChangeTracker.AutoDetectChangesEnabled = true;

            logger.LogInformation("Seeded {Count} orders over {Years} years", total, YearsOfSales);
        }

        private static async Task<int> SaveBatchAsync(ApplicationDbContext db, List<OrderHeader> batch)
        {
            if (batch.Count == 0) return 0;

            db.OrderHeaders.AddRange(batch);
            await db.SaveChangesAsync();
            db.ChangeTracker.Clear();

            int count = batch.Count;
            batch.Clear();
            return count;
        }

        private static OrderHeader BuildOrder(
            Random random,
            ApplicationUser customer,
            List<Product> products,
            double[] productWeights,
            DateTime orderDate,
            DateTime today)
        {
            // How many different books: mostly 1 or 2
            int lines = Pick(random, (1, 55), (2, 28), (3, 12), (4, 5));

            var chosen = new HashSet<int>();
            while (chosen.Count < lines)
            {
                chosen.Add(WeightedIndex(random, productWeights));
            }

            var details = chosen.Select(index => new OrderDetails
            {
                ProductID = products[index].Id,
                Count = Pick(random, (1, 85), (2, 12), (3, 3)),
                Price = products[index].Price
            }).ToList();

            var (orderStatus, paymentStatus) = PickStatus(random, (today - orderDate.Date).Days);

            var order = new OrderHeader
            {
                ApplicationUserId = customer.Id,
                Name = customer.Name,
                PhoneNumber = customer.PhoneNumber ?? "+47 400 00 000",
                StreetAddress = customer.StreetAddress ?? "Storgata 1",
                City = customer.City ?? "Stavanger",
                State = customer.State ?? "Rogaland",
                PostalCode = customer.PostalCode ?? "4006",
                OrderDate = orderDate,
                OrderStatus = orderStatus,
                PaymentStatus = paymentStatus,
                OrderDetails = details,
                OrderTotal = details.Sum(d => d.Price * d.Count)
            };

            if (orderStatus == StaticDetails.StatusShipped)
            {
                order.ShippingDate = orderDate.AddDays(random.Next(1, 4));
                order.Carrier = Carriers[random.Next(Carriers.Length)];
                order.TrackingNumber = "3707" + random.NextInt64(10_000_000_000_000, 99_999_999_999_999);
            }

            return order;
        }

        /// <summary>
        /// New orders are still open. Old orders are mostly shipped,
        /// with a few cancellations and refunds like a real shop.
        /// </summary>
        private static (string OrderStatus, string PaymentStatus) PickStatus(Random random, int ageInDays)
        {
            int roll = random.Next(100);

            if (ageInDays <= 2)
            {
                if (roll < 35) return (StaticDetails.StatusPending, StaticDetails.PaymentStatusPending);
                if (roll < 75) return (StaticDetails.StatusApproved, StaticDetails.PaymentStatusApproved);
                return (StaticDetails.StatusInProcess, StaticDetails.PaymentStatusApproved);
            }

            if (ageInDays <= 7)
            {
                if (roll < 25) return (StaticDetails.StatusApproved, StaticDetails.PaymentStatusApproved);
                if (roll < 60) return (StaticDetails.StatusInProcess, StaticDetails.PaymentStatusApproved);
                return (StaticDetails.StatusShipped, StaticDetails.PaymentStatusApproved);
            }

            if (roll < 88) return (StaticDetails.StatusShipped, StaticDetails.PaymentStatusApproved);
            if (roll < 94) return (StaticDetails.StatusCancelled, StaticDetails.PaymentStatusPending);
            return (StaticDetails.StatusRefunded, StaticDetails.PaymentStatusRefunded);
        }

        // People shop most in the evening
        private static int PickHour(Random random) =>
            Pick(random, (8, 3), (10, 6), (12, 9), (14, 8), (16, 9), (18, 13), (19, 15), (20, 16), (21, 13), (22, 8));

        // ------------------------------------------------------------------
        // Small random helpers
        // ------------------------------------------------------------------
        private static int Pick(Random random, params (int Value, int Weight)[] options)
        {
            int roll = random.Next(options.Sum(o => o.Weight));
            foreach (var (value, weight) in options)
            {
                if (roll < weight) return value;
                roll -= weight;
            }
            return options[^1].Value;
        }

        private static int WeightedIndex(Random random, double[] weights)
        {
            double roll = random.NextDouble() * weights.Sum();
            for (int i = 0; i < weights.Length; i++)
            {
                if (roll < weights[i]) return i;
                roll -= weights[i];
            }
            return weights.Length - 1;
        }

        // Random number of events with a given average (Knuth's method)
        private static int Poisson(Random random, double mean)
        {
            double limit = Math.Exp(-mean), product = 1;
            int count = 0;
            do
            {
                count++;
                product *= random.NextDouble();
            } while (product > limit);
            return count - 1;
        }
    }
}