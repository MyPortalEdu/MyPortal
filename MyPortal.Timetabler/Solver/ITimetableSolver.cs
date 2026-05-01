using MyPortal.Timetabler.Models;

namespace MyPortal.Timetabler.Solver;

public interface ITimetableSolver
{
    TimetableOutput Solve(TimetableInput input, SolveOptions? options = null);
}

public record SolveOptions(
    double MaxSeconds = 30.0,
    int? RandomSeed = null,
    bool LogSearch = false,
    bool MaximiseSpread = true);
