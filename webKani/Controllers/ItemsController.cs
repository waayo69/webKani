using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using webKani.Data;
using webKani.Models;
using ClosedXML.Excel; // Install via NuGet (ClosedXML)
using iTextSharp.text; // Install via NuGet (iTextSharp)
using iTextSharp.text.pdf;

namespace webKani.Controllers
{
    public class ItemsController : Controller
    {
        private readonly MyAppContext _context;

        public ItemsController(MyAppContext context)
        {
            _context = context;
        }

        public IActionResult ExportToPDF()
        {
            var books = _context.Items.ToList();
            using (MemoryStream ms = new MemoryStream())
            {
                Document document = new Document(PageSize.A4);
                PdfWriter writer = PdfWriter.GetInstance(document, ms);
                document.Open();

                // Title
                document.Add(new Paragraph("Book List"));
                document.Add(new Paragraph("\n"));

                PdfPTable table = new PdfPTable(4);
                table.AddCell("Title");
                table.AddCell("Author");
                table.AddCell("Genre");
                table.AddCell("Price");

                foreach (var book in books)
                {
                    table.AddCell(book.Title);
                    table.AddCell(book.Author);
                    table.AddCell(book.Genre);
                    table.AddCell(book.Price.ToString());
                }

                document.Add(table);
                document.Close();

                return File(ms.ToArray(), "application/pdf", "Books.pdf");
            }
        }

        public IActionResult ExportToExcel()
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Books");
                var books = _context.Items.ToList();

                // Headers
                worksheet.Cell(1, 1).Value = "Title";
                worksheet.Cell(1, 2).Value = "Author";
                worksheet.Cell(1, 3).Value = "Genre";
                worksheet.Cell(1, 4).Value = "Price";

                // Data
                for (int i = 0; i < books.Count; i++)
                {
                    worksheet.Cell(i + 2, 1).Value = books[i].Title;
                    worksheet.Cell(i + 2, 2).Value = books[i].Author;
                    worksheet.Cell(i + 2, 3).Value = books[i].Genre;
                    worksheet.Cell(i + 2, 4).Value = books[i].Price;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Books.xlsx");
                }
            }
        }

        public async Task<IActionResult> Index(string genre, string search)
        {
            var books = _context.Items.AsQueryable();

            if (!string.IsNullOrEmpty(genre))
                books = books.Where(b => b.Genre == genre);

            if (!string.IsNullOrEmpty(search))
                books = books.Where(b => b.Title.Contains(search) || b.Author.Contains(search));

            // Top 5 most borrowed
            ViewBag.MostBorrowed = await _context.Items.OrderByDescending(b => b.BorrowCount).Take(5).ToListAsync();

            // Genre list for dropdown
            var genres = await _context.Items.Select(b => b.Genre).Distinct().ToListAsync();
            ViewBag.Genres = genres;
            ViewBag.SelectedGenre = genre;
            ViewBag.SearchQuery = search;

            // ✅ Add book count per genre
            var genreCounts = await _context.Items
                .GroupBy(i => i.Genre)
                .Select(g => new { Genre = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.Genre, g => g.Count);

            ViewBag.GenreCounts = genreCounts;

            return View(await books.ToListAsync());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create([Bind("Id,Title,Author,Genre,Price")] Item item)
        {
            if (ModelState.IsValid)
            {
                _context.Add(item);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(item);
        }

        public async Task<IActionResult> Borrow(int id)
        {
            var book = await _context.Items.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }

            book.BorrowCount++; // Increase borrow count
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var item = await _context.Items.FirstOrDefaultAsync(i => i.Id == id);
            if (item == null) return NotFound();

            ViewBag.Genres = await _context.Items.Select(i => i.Genre).Distinct().ToListAsync();
            return View(item);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteAll()
        {
            _context.Items.RemoveRange(_context.Items);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Author,Genre,Price")] Item item)
        {
            if (ModelState.IsValid)
            {
                _context.Update(item);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(item);
        }
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.Items.FirstOrDefaultAsync(i => i.Id == id);
            return View(item);
        }
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _context.Items.FirstOrDefaultAsync(i => i.Id == id);
            if (item != null)
            {
                _context.Items.Remove(item);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }


    }

}
