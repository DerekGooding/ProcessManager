using System.Diagnostics;

namespace ProcessManager.Models;

internal static class PageCalculator
{
    public static int CalculateCountOfPages(Process[] processes, int countProcessesInPage)
    {
        var countOfPages = processes.Length / countProcessesInPage;

        if (processes.Length % 20 == 0)
        {
            countOfPages--;
        }

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
        }

        return _page;
    }
}