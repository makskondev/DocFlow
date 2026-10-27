namespace DocFlow.Documents;

/// <summary>Ключи для обмена данными между шагами (DocumentProcess.Bag).</summary>
public static class BagKeys
{
    public const string Sha256 = "sha256";     // хеш PDF, который мы отправили на подпись
    public const string PdfPath = "pdfPath";   // где сейчас лежит PDF
    public const string SigPath = "sigPath";   // где сейчас лежит подпись
}
