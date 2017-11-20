using System;
using System.Linq.Expressions;
using CodeToCaml.SpTypes;

namespace CodeToCaml
{
    public static class SpElement
    {
        private const string NotSupportedErrorMessage = 
            "This method can only be called from within a CAML expression.";

        /// <summary>
        /// Renders the <UserID /> element.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public static int UserID { get; internal set; }

        /// <summary>
        /// Renders the <Today /> element.
        /// </summary>
        /// <returns></returns>
        public static DateTime Today()
        {
            throw new NotSupportedException(NotSupportedErrorMessage);
        }

        /// <summary>
        /// Renders the <Today Offset = "Integer" /> element.
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static DateTime Today(int offset)
        {
            throw new NotSupportedException(NotSupportedErrorMessage);
        }

        /// <summary>
        /// Renders the <Now /> element.
        /// </summary>
        public static DateTime Now { get; internal set; }

        /// <summary>
        /// Renders the <Month /> element.
        /// </summary>
        public static int Month { get; internal set; }

        /// <summary>
        /// Renders the <DateRangesOverlap /> element.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="recurrenceId"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool DateRangesOverlap<T>(
            T entity, 
            Expression<Func<T, object>> start, 
            Expression<Func<T, object>> end, 
            Expression<Func<T, object>> recurrenceId,
            DateRange value)
        {
            throw new NotSupportedException(NotSupportedErrorMessage);
        }

        /// <summary>
        /// Renders the <DateRangesOverlap /> element.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="recurrenceId"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool DateRangesOverlap<T>(
            T entity,
            Expression<Func<T, object>> start,
            Expression<Func<T, object>> end,
            Expression<Func<T, object>> recurrenceId,
            DateTime value)
        {
            throw new NotSupportedException(NotSupportedErrorMessage);
        }

        /// <summary>
        /// Renders the <Membership /> element.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="prop"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool Membership<T>(T entity, Expression<Func<T, object>> prop, MembershipType type)
        {
            throw new NotSupportedException(NotSupportedErrorMessage);
        }
    }
}
