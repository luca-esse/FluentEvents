using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using FluentEvents.Utils;

namespace FluentEvents.Model
{
    /// <summary>
    ///     Represents an event source.
    /// </summary>
    public class SourceModel
    {
        private const BindingFlags HandlerFieldsBindingFlags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField;

        /// <summary>
        ///     The type of the event source.
        /// </summary>
        public Type ClrType { get; }

        /// <summary>
        ///     The list of the event fields present on the <see cref="ClrType"/> and created on this model.
        /// </summary>
        public IEnumerable<SourceModelEventField> EventFields => _eventFields;

        private readonly IList<SourceModelEventField> _eventFields;

        /// <summary>
        ///     Creates a new instance of a <see cref="SourceModel"/>.
        /// </summary>
        /// <param name="clrType"></param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="clrType"/> is <see langword="null"/>.
        /// </exception>
        public SourceModel(Type clrType)
        {
            ClrType = clrType ?? throw new ArgumentNullException(nameof(clrType));
            _eventFields = ClrType
                .GetEvents()
                .Select(CreateEventFieldIfValid)
                .Where(x => x != null)
                .ToArray();
        }

        internal Delegate CreateEventHandler<T>(SourceModelEventField eventField, T handlerAction)
        {
            var invokeMethodInfo = typeof(T).GetMethod(nameof(Action.Invoke));

            var handler = Expression.Lambda(
                    eventField.FieldInfo.FieldType,
                    Expression.Call(
                        Expression.Constant(handlerAction),
                        invokeMethodInfo,
                        eventField.EventHandlerParameters[0]
                    ),
                    eventField.EventHandlerParameters
                )
                .Compile();

            return handler;
        }

        private SourceModelEventField CreateEventFieldIfValid(EventInfo eventInfo)
        {
            var fieldInfo = GetEventFieldInfo(ClrType, eventInfo);
            return SourceModelEventField.CreateIfValid(fieldInfo, eventInfo);
        }

        private static FieldInfo GetEventFieldInfo(Type type, MemberInfo eventInfo)
        {
            return type.GetFieldFromBaseTypesInclusive(eventInfo.Name, HandlerFieldsBindingFlags);
        }
    }
}