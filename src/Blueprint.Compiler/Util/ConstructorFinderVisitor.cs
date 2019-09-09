using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Blueprint.Compiler.Util
{
    internal class ConstructorFinderVisitor<T> : ExpressionVisitorBase
    {
        private readonly Type type;
        private ConstructorInfo constructor;

        public ConstructorFinderVisitor(Type type)
        {
            this.type = type;
        }

        public ConstructorInfo Constructor => constructor;

        protected override NewExpression VisitNew(NewExpression nex)
        {
            if (nex.Type == type)
            {
                constructor = nex.Constructor;
            }

            return base.VisitNew(nex);
        }

        public static ConstructorInfo Find(Expression<Func<T>> expression)
        {
            var finder = new ConstructorFinderVisitor<T>(typeof(T));
            finder.Visit(expression);

            return finder.Constructor;
        }
    }
}