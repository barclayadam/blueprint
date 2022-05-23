namespace Blueprint.Compiler.Tests.Scenarios
{
    public interface IAction<T>
    {
        void DoStuff(T arg1);
    }
    
    public interface IAction<T1, T2>
    {
        void DoStuff(T1 arg1, T2 arg2);
    }

    public interface IBuilds<T>
    {
        T Build();
    }
    
    public interface IReturningAction<TResult, T1>
    {
        TResult Create(T1 arg1);
    }
    
    public interface IReturningAction<TResult, T1, T2>
    {
        TResult Create(T1 arg1, T2 arg2);
    }
    
    public interface IReturningAction<TResult, T1, T2, T3>
    {
        TResult Create(T1 arg1, T2 arg2, T3 arg3);
    }
}
