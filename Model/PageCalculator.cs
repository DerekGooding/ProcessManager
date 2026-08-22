using System.Diagnostics;

namespace Process_manager.Model
{
    internal class PageCalculator
    {
        public static int CalculateCountOfPages(Process[] processes, int countProcessesInPage)
        {
            int countOfPages = processes.Length / countProcessesInPage;
            return countOfPages;
        }

        public static Process[] CalculatePage(Process[] processes, int countProcessesInPage, int currentPage)
        {
            Process[] _page = [ ..processes
                .Skip(countProcessesInPage * currentPage)
                .Take(countProcessesInPage) ];

            return _page;
        }

        public static Process[] FillPage(Process[] page, int countProcessesInPage)
        {
            Array.Resize(ref page, countProcessesInPage);

            return page;
        }
    }
}
