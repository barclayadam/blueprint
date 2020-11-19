using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Blueprint.Compiler.Util
{
    internal static class ReflectionHelper
    {
        public static MethodInfo GetMethod<T>(Expression<Action<T>> expression)
        {
            return new FindMethodVisitor(expression).Method;
        }

        public static PropertyInfo GetProperty<TModel>(Expression<Func<TModel, object>> expression)
        {
            var memberExpression = GetMemberExpression(expression);

            return (PropertyInfo)memberExpression.Member;
        }

        private static MemberExpression GetMemberExpression<TModel, T>(Expression<Func<TModel, T>> expression)
        {
            MemberExpression memberExpression = null;

            if (expression.Body.NodeType == ExpressionType.Convert)
            {
                var body = (UnaryExpression)expression.Body;
                memberExpression = body.Operand as MemberExpression;
            }
            else if (expression.Body.NodeType == ExpressionType.MemberAccess)
            {
                memberExpression = expression.Body as MemberExpression;
            }

            if (memberExpression == null)
            {
                throw new ArgumentException("Not a member access", "member");
            }

            return memberExpression;
        }

        private class FindMethodVisitor : ExpressionVisitorBase
        {
            private MethodInfo _method;

            public FindMethodVisitor(Expression expression)
            {
                this.Visit(expression);
            }

            public MethodInfo Method => this._method;

            protected override Expression VisitMethodCall(MethodCallExpression m)
            {
                this._method = m.Method;
                return m;
            }
        }
    }
}
