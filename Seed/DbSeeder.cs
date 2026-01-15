using BookRatings.MVC.Data;
using BookRatings.MVC.Models;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace BookRatings.MVC.Seed
{
    public static class DbSeeder
    {
        // ---------------- ISBN helpers ----------------
        private static List<string> GetIsbnCandidates(string isbn)
        {
            var list = new List<string>();
            if (string.IsNullOrWhiteSpace(isbn)) return list;

            isbn = isbn.Trim().Replace("-", "").Replace(" ", "").ToUpperInvariant();
            list.Add(isbn);

            if (isbn.Length == 10)
            {
                var as13 = Isbn10To13(isbn);
                if (!string.IsNullOrEmpty(as13)) list.Add(as13);
            }
            else if (isbn.Length == 13 && isbn.StartsWith("978"))
            {
                var as10 = Isbn13To10(isbn);
                if (!string.IsNullOrEmpty(as10)) list.Add(as10);
            }

            return list.Distinct().ToList();
        }

        private static string Isbn10To13(string isbn10)
        {
            if (isbn10.Length != 10) return "";
            if (!isbn10.Substring(0, 9).All(char.IsDigit)) return "";

            var core = "978" + isbn10.Substring(0, 9);
            int sum = 0;
            for (int i = 0; i < 12; i++)
            {
                int digit = core[i] - '0';
                sum += (i % 2 == 0) ? digit : digit * 3;
            }
            int check = (10 - (sum % 10)) % 10;
            return core + check.ToString();
        }

        private static string Isbn13To10(string isbn13)
        {
            if (isbn13.Length != 13) return "";
            if (!isbn13.All(char.IsDigit)) return "";
            if (!isbn13.StartsWith("978")) return "";

            var core9 = isbn13.Substring(3, 9);
            int sum = 0;
            for (int i = 0; i < 9; i++)
                sum += (10 - i) * (core9[i] - '0');

            int check = 11 - (sum % 11);
            char checkChar = check switch
            {
                10 => 'X',
                11 => '0',
                _ => (char)('0' + check)
            };

            return core9 + checkChar;
        }

        private static string NormalizeIsbn(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return "";

            var s = raw.Trim().ToUpperInvariant();

            if (s.EndsWith(".0")) s = s[..^2];

            if (s.Contains('E'))
            {
                if (double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var dv))
                    s = ((long)dv).ToString();
            }

            s = new string(s.Where(ch => char.IsDigit(ch) || ch == 'X').ToArray());

            if (s.Length == 9) s = s.PadLeft(10, '0');
            if (s.Length == 12) s = s.PadLeft(13, '0');

            return s;
        }

        // ---------------- SEED ----------------
        public static void Seed(ApplicationDbContext context, IWebHostEnvironment env)
        {
            var book1Path = Path.Combine(env.ContentRootPath, "SeedData", "Book1.xlsx");
            var book2Path = Path.Combine(env.ContentRootPath, "SeedData", "Book2.xlsx");

            if (!File.Exists(book1Path) || !File.Exists(book2Path))
                throw new Exception($"Nu găsesc Excelurile! Book1: {book1Path}, Book2: {book2Path}");

            // ---------- BOOKS ----------
            using (var wb = new XLWorkbook(book1Path))
            {
                var ws = wb.Worksheet(1);
                var rows = ws.RangeUsed().RowsUsed().Skip(1);

                foreach (var row in rows)
                {
                    var isbn = NormalizeIsbn(row.Cell(1).GetFormattedString());
                    if (string.IsNullOrWhiteSpace(isbn)) continue;

                    var candidates = GetIsbnCandidates(isbn);
                    var existingBookIsbn = context.Books.AsNoTracking()
                        .Where(b => candidates.Contains(b.ISBN))
                        .Select(b => b.ISBN)
                        .FirstOrDefault();

                    if (existingBookIsbn != null)
                        continue;

                    var title = row.Cell(2).GetString().Trim();
                    var authorName = row.Cell(3).GetString().Trim();
                    var yearStr = row.Cell(4).GetString().Trim();
                    var publisherName = row.Cell(5).GetString().Trim();

                    int? year = null;
                    if (int.TryParse(yearStr, out var y)) year = y;

                    if (string.IsNullOrWhiteSpace(authorName)) authorName = "Unknown";
                    if (string.IsNullOrWhiteSpace(publisherName)) publisherName = "Unknown";

                    var authorEntity = context.Authors.FirstOrDefault(a => a.Name == authorName);
                    if (authorEntity == null)
                    {
                        authorEntity = new AuthorEntity { Name = authorName };
                        context.Authors.Add(authorEntity);
                        context.SaveChanges();
                    }

                    var publisherEntity = context.Publishers.FirstOrDefault(p => p.Name == publisherName);
                    if (publisherEntity == null)
                    {
                        publisherEntity = new PublisherEntity { Name = publisherName };
                        context.Publishers.Add(publisherEntity);
                        context.SaveChanges();
                    }

                    context.Books.Add(new Book
                    {
                        ISBN = isbn,
                        Title = title,
                        Year = year,
                        AuthorEntityId = authorEntity.AuthorEntityId,
                        PublisherEntityId = publisherEntity.PublisherEntityId
                    });
                }
            }

            context.SaveChanges();

            // ---------- USERS + REVIEWS ----------
            using (var wb = new XLWorkbook(book2Path))
            {
                var ws = wb.Worksheet(1);
                var rows = ws.RangeUsed().RowsUsed().Skip(1);

                // 1) colectează userIds unice din excel
                var excelUserIds = new HashSet<int>();
                foreach (var row in rows)
                {
                    int uid;
                    if (row.Cell(1).DataType == XLDataType.Number)
                        uid = (int)row.Cell(1).GetDouble();
                    else if (!int.TryParse(row.Cell(1).GetFormattedString().Trim(), out uid))
                        continue;

                    excelUserIds.Add(uid);
                }

                // 2) inserează users lipsă cu IDENTITY_INSERT (ca să păstrezi exact UserId din excel)
                var existingUsers = context.Users.AsNoTracking().Select(u => u.UserId).ToHashSet();
                var missingUsers = excelUserIds.Where(u => !existingUsers.Contains(u)).ToList();

                if (missingUsers.Count > 0)
                {
                    context.Database.OpenConnection();
                    try
                    {
                        context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.Users ON;");

                        foreach (var uid in missingUsers)
                            context.Users.Add(new User { UserId = uid });

                        context.SaveChanges();

                        context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.Users OFF;");
                    }
                    finally
                    {
                        context.Database.CloseConnection();
                    }
                }

                // refă rows (că l-am consumat mai sus)
                ws = wb.Worksheet(1);
                rows = ws.RangeUsed().RowsUsed().Skip(1);

                int total = 0, badUser = 0, badIsbn = 0, badRating = 0, noBook = 0, dup = 0, added = 0;

                foreach (var row in rows)
                {
                    total++;

                    // UserId
                    int userId;
                    if (row.Cell(1).DataType == XLDataType.Number)
                        userId = (int)row.Cell(1).GetDouble();
                    else if (!int.TryParse(row.Cell(1).GetFormattedString().Trim(), out userId))
                    {
                        badUser++;
                        continue;
                    }

                    // ISBN
                    var isbn = NormalizeIsbn(row.Cell(2).GetFormattedString());
                    if (string.IsNullOrWhiteSpace(isbn))
                    {
                        badIsbn++;
                        continue;
                    }

                    // Rating
                    int ratingValue;
                    if (row.Cell(3).DataType == XLDataType.Number)
                        ratingValue = (int)row.Cell(3).GetDouble();
                    else if (!int.TryParse(row.Cell(3).GetFormattedString().Trim(), out ratingValue))
                    {
                        badRating++;
                        continue;
                    }

                    // găsește cartea după isbn 10/13
                    var candidates = GetIsbnCandidates(isbn);
                    var book = context.Books.AsNoTracking()
                        .Where(b => candidates.Contains(b.ISBN))
                        .Select(b => new { b.BookId, b.ISBN })
                        .FirstOrDefault();

                    if (book == null)
                    {
                        noBook++;
                        continue;
                    }

                    // duplicate (un review per user per book)
                    var exists = context.Reviews.AsNoTracking()
                        .Any(r => r.UserId == userId && r.BookId == book.BookId);

                    if (exists)
                    {
                        dup++;
                        continue;
                    }

                    context.Reviews.Add(new Review
                    {
                        UserId = userId,
                        BookId = book.BookId,
                        RatingValue = ratingValue
                    });

                    added++;
                }

                context.SaveChanges();

                Console.WriteLine($"Book2: total={total} badUser={badUser} badIsbn={badIsbn} badRating={badRating} noBook={noBook} dup={dup} added={added}");
            }
        }
    }
}