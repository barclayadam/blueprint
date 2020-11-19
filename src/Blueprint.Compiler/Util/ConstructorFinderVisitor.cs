using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Blueprint.Compiler.Util
{
    internal class ConstructorFinderVisitor<T> : ExpressionVisitorBase
    {
        private readonly Type _type;
        private ConstructorInfo _constructor;

        private ConstructorFinderVisitor(Type type)
        {
            this._type = type;
        }

        public static ConstructorInfo Find(Expression<Func<T>> expression)
        {
            var finder = new ConstructorFinderVisitor<T>(typeof(T));
            finder.Visit(expression);

            return finder._constructor;
        }

        protected override NewExpression VisitNew(NewExpression nex)
        {
            if (nex.Type == this._type)
            {
                this._constructor = nex.Constructor;
            }

            return base.VisitNew(nex);
        }
    }
}
