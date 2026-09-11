#pragma once
#include <algorithm>
#include <cmath>
namespace DusklineRules {
enum class Result { Running, Escaped, Dead, TimedOut };
inline Result Resolve(Result Current, Result Requested, bool Alive, double Now, double Deadline) {
    if(Current!=Result::Running || Requested==Result::Running) return Current;
    if(Now>=Deadline) return Result::TimedOut;
    if(!Alive) return Result::Dead;
    return Requested;
}
inline int WeightCapacity(double FreeWeight,double UnitWeight,int Requested) {
    if(!std::isfinite(FreeWeight)||!std::isfinite(UnitWeight)||UnitWeight<=0||Requested<=0||FreeWeight<=0) return 0;
    const double Count=std::floor(FreeWeight/UnitWeight);
    return static_cast<int>(std::min(static_cast<double>(Requested),std::max(0.0,Count)));
}
inline bool CanExtract(bool Running,bool Alive,bool Inside,int Value,int Required,double Now,double Deadline) {
    return Running&&Alive&&Inside&&Value>=Required&&Now<Deadline;
}
}
