#include "../Source/ProjectDuskline/Game/RunRules.h"
#include <iostream>
#include <limits>
#include <cstdlib>
int main(){
    using namespace DusklineRules; int checks=0;
    auto Check=[&](bool Pass,const char* Name){++checks; if(!Pass){std::cerr<<"FAIL: "<<Name<<"\n"; std::exit(1);}};
    Check(Resolve(Result::Running,Result::Escaped,true,599.9,600)==Result::Escaped,"before deadline");
    Check(Resolve(Result::Running,Result::Escaped,true,600,600)==Result::TimedOut,"exact tie");
    Check(Resolve(Result::Running,Result::Escaped,false,599,600)==Result::Dead,"death beats extraction");
    Check(Resolve(Result::Running,Result::Dead,false,600,600)==Result::TimedOut,"death timeout tie");
    Check(Resolve(Result::Escaped,Result::Dead,false,610,600)==Result::Escaped,"terminal idempotence");
    Check(Resolve(Result::Dead,Result::Escaped,true,5,600)==Result::Dead,"death stays terminal");
    Check(Resolve(Result::Running,Result::Running,false,700,600)==Result::Running,"no terminal request");
    Check(WeightCapacity(5,2,10)==2,"fractional capacity");
    Check(WeightCapacity(10,2,2)==2,"requested limit");
    Check(WeightCapacity(0,1,1)==0,"full weight");
    Check(WeightCapacity(-1,1,1)==0,"overweight");
    Check(WeightCapacity(10,0,2)==0,"zero weight invalid");
    Check(WeightCapacity(10,-1,2)==0,"negative weight invalid");
    Check(WeightCapacity(10,std::numeric_limits<double>::quiet_NaN(),2)==0,"nan rejected");
    Check(CanExtract(true,true,true,180,180,599,600),"threshold equality");
    Check(!CanExtract(true,true,true,179,180,599,600),"insufficient value");
    Check(!CanExtract(true,true,false,180,180,599,600),"outside zone");
    Check(!CanExtract(true,false,true,180,180,599,600),"dead player");
    Check(!CanExtract(false,true,true,180,180,599,600),"finished run");
    Check(!CanExtract(true,true,true,180,180,600,600),"deadline reached");
    std::cout<<checks<<" rule checks passed\n"; return 0;
}
