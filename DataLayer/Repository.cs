using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Xml.Linq;

namespace DataLayer
{
    public class Repository<T> where T : class
    {
        private class ExpressionFlatter
        {
            private class WalkExpression : ExpressionVisitor
            {
                private readonly Action<string> newMember;
                private readonly Action newMethod;

                public WalkExpression(Action<string> newMember, Action newMethod)
                {
                    this.newMember = newMember;
                    this.newMethod = newMethod;
                }

                protected override Expression VisitMember(MemberExpression node)
                {
                    newMember.Invoke(node.Member.Name);
                    return base.VisitMember(node);
                }

                protected override Expression VisitLambda<T2>(Expression<T2> node)
                {
                    newMethod.Invoke();
                    return base.VisitLambda(node);
                }
            }

            private readonly WalkExpression we;

            private string lastQuery = string.Empty;
            private string buffer = string.Empty;

            public ExpressionFlatter()
            {
                we = new WalkExpression(NewMember, NewMethod);
            }

            private void NewMethod()
            {
                if (buffer.Length == 0) return;
                if (lastQuery.Length == 0) lastQuery = buffer;
                else lastQuery += $".{buffer}";
                buffer = string.Empty;

                Console.WriteLine($"met: {buffer}");
            }

            private void NewMember(string memberName)
            {
                if (buffer.Length == 0) buffer = memberName;
                else buffer = $"{memberName}.{buffer}";
            }

            public string FlattenExpression(Expression<Func<T, object>> expression)
            {
                lastQuery = string.Empty;
                we.Visit(expression);
                NewMethod();
                return lastQuery;
            }
        }

        private readonly ExpressionFlatter ef = new ExpressionFlatter();

        private readonly AppDbContext appDbContext;
        public Repository(AppDbContext appDbContext)
        {
            this.appDbContext = appDbContext;
        }

        public void Add(T entity)
        {
            appDbContext.Set<T>().Add(entity);
        }

        public IEnumerable<T> FindAll(params Expression<Func<T, object>>[] includes) 
        {
            return Include(includes).ToList();
        }

        public IEnumerable<T> FindAll(Func<T, bool> predicate, params Expression<Func<T, object>>[] includes)
        {
            return Include(includes).Where(predicate).ToList();
        }

        public T Find(Func<T, bool> predicate, params Expression<Func<T, object>>[] includes)
        {
            return Include(includes).FirstOrDefault(predicate);
        }

        public void Remove(T entity)
        {
            appDbContext.Set<T>().Remove(entity);
        }

        private IQueryable<T> Include(params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = appDbContext.Set<T>();

            if (includes is null) return query;

            foreach (var include in includes)
                query = query.Include(ef.FlattenExpression(include));

            return query;
        }
    }
}
