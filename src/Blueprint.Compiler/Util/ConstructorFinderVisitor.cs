using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Blueprint.Compiler.Util
{
    internal class ConstructorFinderVisitor<T> : ExpressionVisitorBase
    {
        private readonly Type type;
        private ConstructorInfo constructor;

        private ConstructorFinderVisitor(Type type)
        {
            this.type = type;
        }

        public static ConstructorInfo Find(Expression<Func<T>> expression)
        {
            var finder = new ConstructorFinderVisitor<T>(typeof(T));
            finder.Visit(expression);

            return finder.constructor;
        }

        protected override NewExpression VisitNew(NewExpression nex)
        {
            if (nex.Type == type)
            {
                constructor = nex.Constructor;
            }

            return base.VisitNew(nex);
        }
    }
}
