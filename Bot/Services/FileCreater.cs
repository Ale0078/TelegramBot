using Aspose.Pdf;
using Aspose.Pdf.Text;

using Bot.Models;

namespace Bot.Services
{
    public static class FileCreater
    {
        public static Stream GetUsersAsPDF(IEnumerable<ChatUser> users) 
        {
            using Document doc = new();

            doc.PageLayout = PageLayout.SinglePage;
            doc.PageMode = PageMode.FullScreen;

            doc.Pages.Add();

            Table table = new Table();

            table.Alignment = HorizontalAlignment.Center;
            table.DefaultCellPadding = new MarginInfo(5, 5, 5, 5);
            table.Border = new BorderInfo(BorderSide.All, .5f, Color.FromRgb(System.Drawing.Color.LightGray));
            table.DefaultCellBorder = new BorderInfo(BorderSide.All, .5f, Color.FromRgb(System.Drawing.Color.LightGray));

            Row head = table.Rows.Add();

            head.Cells.Add("Имя");
            head.Cells.Add("Фамилия");
            head.Cells.Add("Тлеграмм");
            head.Cells.Add("Откуда узнал");

            foreach (ChatUser user in users)
            {
                Row row = table.Rows.Add();

                row.Cells.Add(user.FirstName);
                row.Cells.Add(user.Surname);
                row.Cells.Add(user.UserName);
                row.Cells.Add(user.From.ToString());
            }

            TextFragment textFragment = new TextFragment("Пользователи телеграмм бота");

            doc.Pages[1].Paragraphs.Add(textFragment);
            doc.Pages[1].Paragraphs.Add(table);

            MemoryStream ms = new MemoryStream();

            doc.Save(ms, SaveFormat.Pdf);

            return ms;
        }
    }
}
