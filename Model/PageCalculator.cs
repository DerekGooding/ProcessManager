using System.Diagnostics;

namespace Process_manager.Model
{
    internal class PageCalculator
    {
        public static int CalculateCountOfPages(Process[] processes, int countProcessesInPage)
        {
            int countOfPages = processes.Length / countProcessesInPage;

            if (processes.Length % 20 != 0)
            {
                countOfPages++;
            }

            return countOfPages;
        }

        public static Process[] CalculatePage(Process[] processes, int countProcessesInPage, int currentPage)
        {
            Process[] _page = [ ..processes
                .Skip(countProcessesInPage * currentPage)
                .Take(countProcessesInPage) ];

            return _page;
        }
    }
}
