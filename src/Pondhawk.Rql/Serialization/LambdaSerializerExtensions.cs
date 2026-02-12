using System.Linq.Expressions;
using System.Reflection;
using CommunityToolkit.Diagnostics;
using Pondhawk.Rql.Builder;

namespace Pondhawk.Rql.Serialization
{


    public static class LambdaSerializerExtensions
    {

        private static readonly MethodInfo StartsWithMethod = typeof(string).GetMethod("StartsWith", [typeof(string)])!;
        private static readonly MethodInfo StartsWithCiMethod = typeof(string).GetMethod("StartsWith", [typeof(string), typeof(StringComparison)])!;
        private static readonly MethodInfo ContainsMethod = typeof(string).GetMethod("Contains", [typeof(string)])!;
        private static readonly MethodInfo ContainsCiMethod = typeof(string).GetMethod("Contains", [typeof(string), typeof(StringComparison)])!;
        private static readonly MethodInfo ListContainsMethod = typeof(List<object>).GetMethod("Contains", [typeof(object)])!;


        public static Func<TEntity, bool> ToLambda<TEntity>(  this IRqlFilter<TEntity> filter, bool insensitive = false) where TEntity : class
        {

            Guard.IsNotNull(filter);

            var expression = filter.ToExpression(insensitive);
            var lambda = expression.Compile();

            return lambda;

        }

        public static Expression<Func<TEntity, bool>> ToExpression<TEntity>(  this IRqlFilter<TEntity> filter, bool insensitive=false ) where TEntity : class
        {

            Guard.IsNotNull(filter);

            var entity = Expression.Parameter(typeof(TEntity), "e");

            Expression running = null;

            foreach( var predicate in filter.Criteria )
            {

                switch (predicate.Operator)
                {

                    case RqlOperator.Equals:
                        running = BuildComparison( running, entity, predicate, Expression.Equal );
                        break;
                    case RqlOperator.NotEquals:
                        running = BuildComparison( running, entity, predicate, Expression.NotEqual );
                        break;
                    case RqlOperator.LesserThan:
                        running = BuildComparison( running, entity, predicate, Expression.LessThan );
                        break;
                    case RqlOperator.GreaterThan:
                        running = BuildComparison( running, entity, predicate, Expression.GreaterThan );
                        break;
                    case RqlOperator.LesserThanOrEqual:
                        running = BuildComparison( running, entity, predicate, Expression.LessThanOrEqual );
                        break;
                    case RqlOperator.GreaterThanOrEqual:
                        running = BuildComparison( running, entity, predicate, Expression.GreaterThanOrEqual );
                        break;
                    case RqlOperator.StartsWith when predicate.DataType == typeof(string) && insensitive:
                        running = BuildStartsWithCi(running, entity, predicate);
                        break;
                    case RqlOperator.StartsWith when predicate.DataType == typeof(string) && !insensitive:
                        running = BuildStartsWith(running, entity, predicate);
                        break;
                    case RqlOperator.Contains when predicate.DataType == typeof(string) && insensitive:
                        running = BuildContainsCi(running, entity, predicate);
                        break;
                    case RqlOperator.Contains when predicate.DataType == typeof(string) && !insensitive:
                        running = BuildContains(running, entity, predicate);
                        break;
                    case RqlOperator.Between:
                        running = BuildBetween( running, entity, predicate );
                        break;
                    case RqlOperator.In:
                        running = BuildIn(running, entity, predicate);
                        break;
                    case RqlOperator.NotIn:
                        running = BuildNotIn(running, entity, predicate);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();

                }

            }


            if( running == null )
            {
                Expression<Func<TEntity, bool>> none = _=> true;
                return none;
            }

            return Expression.Lambda<Func<TEntity,bool>>( running, entity );

        }

        private static (Expression left, Expression right) BuildOperands(Expression entity, string name, Type dataType, object value)
        {

            var left = Expression.Property(entity, name);
            ConstantExpression right;

            var prop  = (PropertyInfo)left.Member;
            if (prop.PropertyType != dataType)
            {
                var conv = Convert.ChangeType(value, prop.PropertyType);
                right = Expression.Constant(conv, prop.PropertyType);
            }
            else
                right = Expression.Constant(value, dataType);

            return (left, right);

        }

        private static (Expression left, IEnumerable<Expression> right) BuildOperandsInsensitive(Expression entity, string name, string value )
        {

            var left  = Expression.Property(entity, name);
            var right = new List<Expression> {Expression.Constant(value, typeof(string)), Expression.Constant(StringComparison.InvariantCultureIgnoreCase, typeof(StringComparison)) };

            return (left, right);

        }

        private static Expression BuildComparison(Expression running, Expression entity, IRqlPredicate predicate, Func<Expression, Expression, BinaryExpression> factory)
        {

            var (left, right) = BuildOperands(entity, predicate.Target.Name, predicate.DataType, predicate.Values[0]);

            var exp = factory(left, right);

            if (running is null)
                return exp;

            return Expression.AndAlso(running, exp);

        }

        private static Expression BuildBetween(Expression running, Expression entity, IRqlPredicate predicate)
        {

            var (leftFrom, rightFrom) = BuildOperands(entity, predicate.Target.Name, predicate.DataType, predicate.Values[0]);

            var from = Expression.GreaterThanOrEqual(leftFrom, rightFrom);

            var (leftTo, rightTo) = BuildOperands(entity, predicate.Target.Name, predicate.DataType, predicate.Values[1]);

            var to = Expression.LessThanOrEqual(leftTo, rightTo);

            var exp = Expression.AndAlso(from,to);

            if (running is null)
                return exp;

            return Expression.AndAlso(running, exp);

        }

        private static Expression BuildStartsWith( Expression running, Expression entity, IRqlPredicate predicate )
        {

            var (left, right) = BuildOperands( entity, predicate.Target.Name, typeof(string), predicate.Values[0].ToString() );

            var exp = Expression.Call(left, StartsWithMethod, right);

            if (running is null)
                return exp;

            return Expression.AndAlso(running, exp);

        }

        private static Expression BuildStartsWithCi(Expression running, Expression entity, IRqlPredicate predicate)
        {

            var (left, right) = BuildOperandsInsensitive(entity, predicate.Target.Name, predicate.Values[0].ToString());

            var exp = Expression.Call(left, StartsWithCiMethod, right);

            if (running is null)
                return exp;

            return Expression.AndAlso(running, exp);

        }

        private static Expression BuildContains(Expression running, Expression entity, IRqlPredicate predicate)
        {

            var (left, right) = BuildOperands(entity, predicate.Target.Name, typeof(string),predicate.Values[0].ToString());

            var exp = Expression.Call( left, ContainsMethod, right );

            if (running is null)
                return exp;

            return Expression.AndAlso(running, exp);

        }

        private static Expression BuildContainsCi(Expression running, Expression entity, IRqlPredicate predicate)
        {

            var (left, right) = BuildOperandsInsensitive(entity, predicate.Target.Name, predicate.Values[0].ToString());

            var exp = Expression.Call(left, ContainsCiMethod, right);

            if (running is null)
                return exp;

            return Expression.AndAlso(running, exp);

        }

        private static Expression BuildIn( Expression running, Expression entity, IRqlPredicate predicate )
        {

            var left  = Expression.Constant(new List<object>(predicate.Values), typeof(List<object>));
            var cand  = Expression.Property(entity, predicate.Target.Name);
            var right = Expression.Convert(cand, typeof(object));

            var exp = Expression.Call(left, ListContainsMethod, right);

            if (running is null)
                return exp;

            return Expression.AndAlso(running, exp);

        }

        private static Expression BuildNotIn(Expression running, Expression entity, IRqlPredicate predicate)
        {

            var left  = Expression.Constant(new List<object>(predicate.Values), typeof(List<object>));
            var cand  = Expression.Property(entity, predicate.Target.Name);
            var right = Expression.Convert(cand, typeof(object));

            var found = Expression.Call(left, ListContainsMethod, right);
            var exp = Expression.Not(found);

            if (running is null)
                return exp;

            return Expression.AndAlso(running, exp);

        }


    }








}
