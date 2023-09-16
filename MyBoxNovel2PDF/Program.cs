using HtmlAgilityPack;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Navigation;
using iText.Layout;
using iText.Layout.Element;

string baseURL = "https://myboxnovel.com/book/";
int startChapter = 1;
int endChapter;

Console.WriteLine($"Please provide your https://myboxnovel.com/book/YOUR-DESIRED-NOVEL-boxnovel/chapter-1/");
Console.Write($"Your novel name (eg. versatile mage):");
var novel = Console.ReadLine();
if (novel == null)
{
    Console.WriteLine("Nothing to convert, exiting");
    Console.ReadKey();
    return;
}
novel = novel.ToLowerInvariant();
Console.WriteLine("Type chapter range or enter to skip (generate all)");
Console.Write("From chapter: ");
var begin = Console.ReadLine();
if (begin != string.Empty)
{
    if (!int.TryParse(begin, out startChapter))
    {
        Console.WriteLine("You`re locked out, try again in 5 min and type a number dude");
        return;
    }
    Console.Write("To chapter: ");
    if(!int.TryParse(Console.ReadLine(),out endChapter)){
        Console.WriteLine("You`re locked out, try again in 5 min and type a number dude");
        return;
    }
}
else
{
    Console.WriteLine("Skipped, generating everythin i can find, brace yourself, it`s gonna be a long ride");
    Console.WriteLine("Press Ctrl + C to stop generating");
}
try
{
    // Create an HTML document
    var web = new HtmlWeb();
    string searcURL = $"{baseURL}{novel.Replace(' ', '-').ToLowerInvariant()}-boxnovel/chapter-{startChapter}/";
    var doc = web.Load(searcURL);
    HtmlNode? contentNodes = GetContent(doc);
    // Create a PDF document
    var pdfPath = $"output_{novel}.pdf";
    var pdfWriter = new PdfWriter(pdfPath);
    var pdfDocument = new PdfDocument(pdfWriter);
    var pdf = new Document(pdfDocument);

    // Stop mechanism
    bool continueQuerying = true;
    Console.CancelKeyPress += (sender, e) =>
    {
        Console.WriteLine("cancelled");
        e.Cancel = true; // Prevent the application from terminating
        continueQuerying = false; // Set a flag to stop querying
    };


    while (contentNodes != null && continueQuerying)
    {
        pdf.Add(new Paragraph($"Chapter {startChapter}"));
        var outline = pdfDocument.GetOutlines(false).AddOutline($"Chapter {startChapter}");
        outline.AddDestination(PdfExplicitDestination.CreateFit(pdfDocument.GetLastPage()));

        searcURL = $"{baseURL}{novel.Replace(' ', '-').ToLowerInvariant()}-boxnovel/chapter-{startChapter++}/";
        doc = web.Load(searcURL);
        contentNodes = GetContent(doc);
        if(contentNodes == null)
        {
            Console.WriteLine($"Content not found for {searcURL}");
            break;
        }
        var paragraphNodes = contentNodes.SelectNodes(".//p");
        if (paragraphNodes == null)
        {
            paragraphNodes = contentNodes.SelectNodes(".//strong"); 
            if (paragraphNodes == null)
            {
                Console.WriteLine($"Content not found for {searcURL}");
                break;
            }
        }
        Console.WriteLine($"Content found for {searcURL}");
        foreach (var node in paragraphNodes)
        {
            //comment this if you don`t want to remove old published chapter credits
            if (node.InnerText.Equals("* * *"))
            {
                break;
            }
            pdf.Add(new Paragraph(System.Net.WebUtility.HtmlDecode(node.InnerText)));
        }
        pdf.Add(new AreaBreak());
    }
    pdf.Close();
    Console.WriteLine($"PDF generated for {searcURL}:");
}
catch (Exception ex)
{
    Console.WriteLine("Error found: " + ex.Message + Environment.NewLine + ex.InnerException + Environment.NewLine + ex.StackTrace);
}

static HtmlNode GetContent(HtmlDocument doc)
{
    var contentNodes = doc.DocumentNode.SelectSingleNode("//div[@class='cha-words']");
    if (contentNodes == null)
    {
        contentNodes = doc.DocumentNode.SelectSingleNode("//div[@class='text-left']");
    }

    return contentNodes;
}