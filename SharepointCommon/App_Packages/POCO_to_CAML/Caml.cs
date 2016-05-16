using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Xml;
using CodeToCaml.SpTypes;

// ReSharper disable MergeConditionalExpression

namespace CodeToCaml
{
    public class Caml<T> 
    {
        public SpViewScope ViewScope { private get; set; }

        private Expression<Func<T, object>> _select;
        private Expression<Func<T, bool>> _where;
        private BinaryExpression _whereBe;
        private int _take;
        private LambdaExpression _groupBy;
        private bool _collapse;
        private readonly List<Sort> _order;

        private const string PropertyExpressionNotSupported =
            "For property expression, specify the right value e.g. 'f => f.IsActive == true' instead of 'f => f.IsActive'";

        public Caml()
        {
            _order = new List<Sort>();
        }

        public Caml<T> Select(Expression<Func<T, object>> select)
        {
            _select = select;
            return this;
        }

        public Caml<T> Where(Expression<Func<T, bool>> where)
        {
            _where = where;
            return this;
        }

        public Caml<T> AndAlso(Expression<Func<T, bool>> where)
        {
            if (_where == null)
                _where = where;
            else
                _whereBe = Expression.AndAlso(_whereBe ?? _where.Body, @where.Body);

            return this;
        }

        public Caml<T> OrElse(Expression<Func<T, bool>> where)
        {
            if (_where == null)
                _where = where;
            else
                _whereBe = Expression.OrElse(_whereBe ?? _where.Body, @where.Body);

            return this;
        }

        public Caml<T> Take(int rows)
        {
            _take = rows;
            return this;
        }

        public Caml<T> GroupBy<TProp>(Expression<Func<T, TProp>> keySelector)
        {
            _groupBy = keySelector;
            return this;
        }

        public Caml<T> GroupByCollapse<TProp>(Expression<Func<T, TProp>> keySelector)
        {
            _groupBy = keySelector;
            _collapse = true;
            return this;
        }

        public Caml<T> OrderBy<TProp>(Expression<Func<T, TProp>> keySelector)
        {
            _order.Add(new Sort { Expression = keySelector, Ascending = true });
            return this;
        }

        public Caml<T> OrderByDescending<TProp>(Expression<Func<T, TProp>> keySelector)
        {
            _order.Add(new Sort { Expression = keySelector, Ascending = false });
            return this;
        }

        public Caml<T> ThenBy<TProp>(Expression<Func<T, TProp>> keySelector)
        {
            _order.Add(new Sort { Expression = keySelector, Ascending = true });
            return this;
        }

        public Caml<T> ThenByDescending<TProp>(Expression<Func<T, TProp>> keySelector)
        {
            _order.Add(new Sort { Expression = keySelector, Ascending = false });
            return this;
        }

        public override string ToString()
        {
            string query;

            using (var sw = new StringWriter())
            {
                using (var writer = XmlWriter.Create(sw))
                {
                    writer.WriteStartElement(Tags.View);

                    if (ViewScope != SpViewScope.Default)
                        writer.WriteAttributeString(Tags.Scope, ViewScope.ToString());

                    // Select
                    if (_select != null)
                    {
                        writer.WriteStartElement(Tags.ViewFields);
                        BuildSelect(writer, _select.Body);
                        writer.WriteEndElement();
                    }

                    // Where, Group By and Order
                    writer.WriteStartElement(Tags.Query);

                    if (_where != null)
                    {
                        writer.WriteStartElement(Tags.Where);

                        if (_whereBe == null)
                            BuildWhere(writer, _where.Body);
                        else
                            BuildWhere(writer, _whereBe);

                        writer.WriteEndElement();
                    }

                    if (_groupBy != null)
                        BuildGroupBy(writer, _groupBy.Body, _collapse);

                    if (_order.Any())
                    {
                        writer.WriteStartElement(Tags.OrderBy);
                        foreach (var sort in _order)
                        {
                            BuildOrder(writer, sort.Expression.Body, sort.Ascending);
                        }
                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();

                    // Take
                    if (_take > 0)
                    {
                        writer.WriteStartElement(Tags.RowLimit);
                        writer.WriteValue(_take);
                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();
                }

                var xml = new XmlDocument();
                xml.LoadXml(sw.ToString());

                if (xml.FirstChild.NodeType == XmlNodeType.XmlDeclaration)
                    xml.RemoveChild(xml.FirstChild);

                query = xml.InnerXml;
            }

            return query;
        }

        private static void BuildGroupBy(XmlWriter writer, Expression e, bool collapse)
        {
            var me = e as MemberExpression;

            if (me == null)
                throw new NotSupportedException();

            writer.WriteStartElement(Tags.GroupBy);

            if (collapse)
                writer.WriteAttributeString(Tags.Collapse, bool.TrueString);

            writer.WriteStartElement(Tags.FieldRef);
            writer.WriteAttributeString(Tags.Name, GetFieldName(me));
            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        private static void BuildOrder(XmlWriter writer, Expression e, bool ascending)
        {
            var me = e as MemberExpression;

            if (me == null)
                throw new NotSupportedException();

            writer.WriteStartElement(Tags.FieldRef);
            writer.WriteAttributeString(Tags.Name, GetFieldName(me));
            writer.WriteAttributeString(Tags.Ascending, ascending.ToString());
            writer.WriteEndElement();
        }

        private static void BuildSelect(XmlWriter writer, Expression e)
        {
            //// CHANGED
            if (e.NodeType == ExpressionType.Convert)
            {
                var convert = e as UnaryExpression;
                if (convert == null)
                    throw new InvalidOperationException("Cannot understand query.");

                var ne = convert.Operand as NewExpression;
                var me = convert.Operand as MemberExpression;

                IEnumerable<MemberExpression> mes = null;

                if (ne != null)
                {
                    mes = ne.Arguments.Select(a => a as MemberExpression);
                }
                else if (me != null)
                {
                    mes = new List<MemberExpression> { me };
                }
                else
                {
                    throw new InvalidOperationException("Cannot understand query.");
                }

                foreach (var m in mes)
                {
                    if (m == null)
                        throw new NotSupportedException();

                    writer.WriteStartElement(Tags.FieldRef);
                    writer.WriteAttributeString(Tags.Name, GetFieldName(m));
                    writer.WriteEndElement();
                }

            }
            //// CHANGED
        }

        private static void BuildWhere(XmlWriter writer, Expression e)
        {
            var be = e as BinaryExpression;

            if (be != null)
            {
                BuildBinary(writer, be);
                return;
            }

            // Add support for method call (e.g. Contains, StartsWith, etc.) expression.
            var mce = e as MethodCallExpression;

            // TODO: Add support for boolean property expression (e.g. f => f.IsActive).
            if (mce == null) throw new NotSupportedException(PropertyExpressionNotSupported);
            BuildMethodCall(writer, mce);
        }

        private static void BuildBinary(XmlWriter writer, BinaryExpression be)
        {
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (be.NodeType)
            {
                case ExpressionType.AndAlso:

                    writer.WriteStartElement(Tags.And);
                    BuildWhere(writer, be.Left);
                    BuildWhere(writer, be.Right);
                    writer.WriteEndElement();
                    break;

                case ExpressionType.OrElse:

                    writer.WriteStartElement(Tags.Or);
                    BuildWhere(writer, be.Left);
                    BuildWhere(writer, be.Right);
                    writer.WriteEndElement();
                    break;

                default:
                    BuildFields(writer, be);
                    break;
            }
        }

        private static void BuildMethodCall(XmlWriter writer, MethodCallExpression mce)
        {
            switch (mce.Method.Name)
            {
                case "Contains":

                    if (mce.Method.DeclaringType != null && mce.Method.DeclaringType.FullName == typeof(string).FullName)
                        WriteMethodFieldValue(writer, mce, Tags.Contains);
                    else if (mce.Method.DeclaringType != null && mce.Method.DeclaringType.FullName == typeof(Enumerable).FullName)
                        WriteContainsList(writer, mce);
                    else
                        throw new NotSupportedException();

                    break;

                case "StartsWith":
                    WriteMethodFieldValue(writer, mce, Tags.BeginsWith);
                    break;

                case "Includes":

                    if (mce.Method.DeclaringType != null && mce.Method.DeclaringType.FullName != typeof(SpExtension).FullName)
                        throw new NotSupportedException();

                    WriteIncludeFieldValue(writer, mce, Tags.Includes);

                    break;

                case "NotIncludes":

                    if (mce.Method.DeclaringType != null && mce.Method.DeclaringType.FullName != typeof(SpExtension).FullName)
                        throw new NotSupportedException();

                    WriteIncludeFieldValue(writer, mce, Tags.NotIncludes);
                    
                    break;

                case "Membership":

                    if (mce.Method.DeclaringType != null && mce.Method.DeclaringType.FullName != typeof(SpElement).FullName)
                        throw new NotSupportedException();

                    var membershipProp = ((LambdaExpression) ((UnaryExpression) mce.Arguments[1]).Operand).Body;
                    var membershipType = (MembershipType)GetFieldValue(mce.Arguments[2]);

                    writer.WriteStartElement(Tags.Membership);

                    switch (membershipType)
                    {
                        case MembershipType.SpWebAllUsers:
                            writer.WriteAttributeString(Tags.Name, Tags.SpWebAllUsers);
                            break;
                        case MembershipType.SpGroup:
                            writer.WriteAttributeString(Tags.Name, Tags.SpGroup);
                            break;
                        case MembershipType.SpWebGroups:
                            writer.WriteAttributeString(Tags.Name, Tags.SpWebGroups);
                            break;
                        case MembershipType.CurrentUserGroups:
                            writer.WriteAttributeString(Tags.Name, Tags.CurrentUserGroups);
                            break;
                        case MembershipType.SpWebUsers:
                            writer.WriteAttributeString(Tags.Name, Tags.SpWebUsers);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    writer.WriteStartElement(Tags.FieldRef);
                    writer.WriteAttributeString(Tags.Name, GetFieldName(membershipProp));
                    writer.WriteEndElement();

                    writer.WriteEndElement();

                    break;

                case "DateRangesOverlap":

                    if (mce.Method.DeclaringType != null && mce.Method.DeclaringType.FullName != typeof(SpElement).FullName)
                        throw new NotSupportedException();

                    writer.WriteStartElement(Tags.DateRangesOverlap);

                    // ReSharper disable once LoopCanBeConvertedToQuery
                    foreach (
                        var e in mce.Arguments
                            .Skip(1)
                            .Take(3)
                            .Select(exp => exp as UnaryExpression)
                            .Select(uexp => ((LambdaExpression) uexp.Operand).Body))
                    {
                        var ue = e as UnaryExpression;
                        var pe = e as MemberExpression;

                        if (ue == null && pe == null)
                            throw new InvalidOperationException();

                        var prop = ue != null ? ue.Operand as MemberExpression : pe;

                        if (prop == null)
                            throw new InvalidOperationException();

                        writer.WriteStartElement(Tags.FieldRef);
                        writer.WriteAttributeString(Tags.Name, GetFieldName(prop));
                        writer.WriteEndElement();
                    }

                    writer.WriteStartElement(Tags.Value);
                    writer.WriteAttributeString(Tags.Type, Constant.SpDateTime);

                    var value = mce.Arguments[4];

                    if (value.Type.FullName == typeof(DateRange).FullName)
                    {
                        writer.WriteStartElement(((ConstantExpression)value).Value.ToString());
                        writer.WriteEndElement();
                    }
                    else
                        writer.WriteValue(GetFieldValue(value));

                    writer.WriteEndElement();
                    writer.WriteEndElement();

                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void BuildFields(XmlWriter writer, BinaryExpression be)
        {
            var type = be.Right.Type;
            var isNullable = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);

            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (be.NodeType)
            {
                case ExpressionType.Equal:

                    var ceEq = be.Right as ConstantExpression;

                    if ((ceEq != null && ceEq.Value == null) /*|| isNullable*/)
                    {
                        writer.WriteStartElement(Tags.IsNull);
                        writer.WriteStartElement(Tags.FieldRef);
                        writer.WriteAttributeString(Tags.Name, GetFieldName(be.Left));
                        writer.WriteEndElement();
                        writer.WriteEndElement();
                    }
                    else
                        WriteFieldValue(writer, be, Tags.Eq);

                    break;

                case ExpressionType.NotEqual:

                    var ceNeq = be.Right as ConstantExpression;

                    if ((ceNeq != null && ceNeq.Value == null) || isNullable)
                    {
                        writer.WriteStartElement(Tags.IsNotNull);
                        writer.WriteStartElement(Tags.FieldRef);
                        writer.WriteAttributeString(Tags.Name, GetFieldName(be.Left));
                        writer.WriteEndElement();
                        writer.WriteEndElement();
                    }
                    else
                        WriteFieldValue(writer, be, Tags.Neq);

                    break;

                case ExpressionType.GreaterThan:
                    WriteFieldValue(writer, be, Tags.Gt);
                    break;

                case ExpressionType.GreaterThanOrEqual:
                    WriteFieldValue(writer, be, Tags.Geq);
                    break;

                case ExpressionType.LessThan:
                    WriteFieldValue(writer, be, Tags.Lt);
                    break;

                case ExpressionType.LessThanOrEqual:
                    WriteFieldValue(writer, be, Tags.Leq);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void WriteFieldValue(XmlWriter writer, BinaryExpression be, string operatorTag)
        {
            writer.WriteStartElement(operatorTag);

            writer.WriteStartElement(Tags.FieldRef);
            writer.WriteAttributeString(Tags.Name, GetFieldName(be.Left));
            writer.WriteEndElement();

            writer.WriteStartElement(Tags.Value);
            writer.WriteAttributeString(Tags.Type, GetFieldType(be.Left));

            var pe = be.Right as MemberExpression;
            var mc = be.Right as MethodCallExpression;
            var spElement = typeof (SpElement).FullName;

            if (pe?.Member.DeclaringType != null && pe.Member.DeclaringType.FullName == spElement)
            {
                writer.WriteStartElement(pe.Member.Name);
                writer.WriteEndElement();
            }
            else if (mc?.Method.DeclaringType != null && mc.Method.DeclaringType.FullName == spElement)
            {
                writer.WriteStartElement(mc.Method.Name);

                if (mc.Arguments.Any())
                    writer.WriteAttributeString(Tags.Offset, mc.Arguments.First().ToString());

                writer.WriteEndElement();
            }
            else
                writer.WriteValue(GetFieldValue(be.Right));

            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        private static void WriteMethodFieldValue(XmlWriter writer, MethodCallExpression mce, string operatorTag)
        {
            writer.WriteStartElement(operatorTag);
            writer.WriteStartElement(Tags.FieldRef);

            var me = (MemberExpression)mce.Object;

            if (me != null)
            {
                writer.WriteAttributeString(Tags.Name, GetFieldName(me));
                writer.WriteEndElement();
            }
            else
                throw new NotSupportedException();

            writer.WriteStartElement(Tags.Value);
            writer.WriteAttributeString(Tags.Type, GetFieldType(me));
            writer.WriteValue(GetFieldValue(mce.Arguments[0]));

            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        private static void WriteContainsList(XmlWriter writer, MethodCallExpression mce)
        {
            writer.WriteStartElement(Tags.In);
            writer.WriteStartElement(Tags.FieldRef);

            var me = (MemberExpression)mce.Arguments[1];

            if (me != null)
            {
                writer.WriteAttributeString(Tags.Name, GetFieldName(me));
                writer.WriteEndElement();
            }
            else
                throw new NotSupportedException();

            var listMe = (MemberExpression)mce.Arguments[0];

            if (listMe == null)
                throw new NotSupportedException();

            var listName = listMe.Member.Name;
            var listValueCe = (ConstantExpression)listMe.Expression;
            var fieldType = GetFieldType(me);
            var type = listValueCe.Value.GetType();
            var fieldInfo = type.GetField(listName);
            var values = fieldInfo.GetValue(listValueCe.Value);
            var list = (IEnumerable)values;

            writer.WriteStartElement(Tags.Values);

            foreach (var i in list)
            {
                writer.WriteStartElement(Tags.Value);
                writer.WriteAttributeString(Tags.Type, fieldType);

                if (i.GetType().IsEnum)
                    writer.WriteValue((int)i);
                else
                    writer.WriteValue(i);

                writer.WriteEndElement();
            }

            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        private static void WriteIncludeFieldValue(XmlWriter writer, MethodCallExpression mce, string operatorTag)
        {
            writer.WriteStartElement(operatorTag);

            writer.WriteStartElement(Tags.FieldRef);
            writer.WriteAttributeString(Tags.Name, GetFieldName(mce.Arguments[0]));
            writer.WriteEndElement();

            writer.WriteStartElement(Tags.Value);
            writer.WriteAttributeString(Tags.Type, GetFieldType(mce.Arguments[0]));
            writer.WriteValue(GetFieldValue(mce.Arguments[1]));
            writer.WriteEndElement();

            writer.WriteEndElement();
        }

        private static string GetFieldName(Expression e)
        {
            var me = e as MemberExpression;
            var ue = e as UnaryExpression;

            if (me == null && !(ue?.Operand is MemberExpression))
                throw new NotSupportedException();

            ////CHANGED
            var member = me != null ? me.Member : ((MemberExpression)ue.Operand).Member;
            var customAttributes = member.GetCustomAttributes(typeof(SharepointCommon.Attributes.FieldAttribute), false);

            if (!customAttributes.Any())
            {
                var name = SharepointCommon.Common.FieldMapper.TranslateToFieldName(member.Name);
                return name;
            }

            var spData = (SharepointCommon.Attributes.FieldAttribute)customAttributes.First();
            ////
            
            return string.IsNullOrEmpty(spData.Name)
                ? member.Name
                : spData.Name.Replace(Constant.Space, Constant.SpaceEncode);
        }

        private static string GetFieldType(Expression e)
        {
            var me = e as MemberExpression;
            var ue = e as UnaryExpression;

            if (me == null && !(ue?.Operand is MemberExpression))
                throw new NotSupportedException();

            var member = me != null ? me.Member : ((MemberExpression)ue.Operand).Member;
            var defaultType = me != null ? me.Type : ue.Operand.Type;
            var customAttributes = member.GetCustomAttributes(typeof(SpDataAttribute), false);

            if (!customAttributes.Any())
                return GetValueType(defaultType);

            var spData = (SpDataAttribute)customAttributes.First();

            return string.IsNullOrEmpty(spData.ValueType)
                ? GetValueType(defaultType)
                : spData.ValueType;
        }

        private static string GetValueType(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                type = type.GetGenericArguments().First();
            }

            switch (type.FullName)
            {
                case Constant.TypeString:
                    return Constant.SpText;

                case Constant.TypeInt32:
                    return Constant.SpInteger;

                case Constant.TypeDecimal:
                case Constant.TypeSingle:
                case Constant.TypeDouble:
                    return Constant.SpNumber;

                case Constant.TypeDateTime:
                    return Constant.SpDateTime;

                case Constant.TypeBoolean:
                    return Constant.SpBoolean;

                default:
                    throw new NotSupportedException();
            }
        }

        private static object GetFieldValue(Expression e)
        {
            var type = e.Type;

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                type = type.GetGenericArguments().First();
            }

            if (type.FullName == typeof(bool).FullName)
            {
                var ce = e as ConstantExpression;

                if (ce == null)
                    throw new InvalidOperationException();

                return (bool) ce.Value ? 1 : 0;
            }

            var compiled = Expression.Lambda(e).Compile();
            return compiled.DynamicInvoke();
        }
    }
}
