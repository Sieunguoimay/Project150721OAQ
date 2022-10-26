namespace Framework
{
    public abstract class InnerClass<TContext>
    {
        protected readonly TContext Context;

        protected InnerClass(TContext context)
        {
            Context = context;
        }
    }
}