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

            if (_page.Length < countProcessesInPage)
            {
                Array.Resize(ref _page, countProcessesInPage);

                for (int i = _page.Length - 1; i < countProcessesInPage; i++)
                {
                    _page[i] = new Process();
                }
            }

            return _page;
        }
    }
}
