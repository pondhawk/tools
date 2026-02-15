/*
The MIT License (MIT)

Copyright (c) 2024 Pond Hawk Technologies Inc.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Pondhawk.Watch.Framework.Utilities;

namespace Pondhawk.Watch.Framework.Serializers
{
    public class NewtonsoftObjectSerializer : IObjectSerializer
    {
        public static readonly NewtonsoftObjectSerializer Instance = new NewtonsoftObjectSerializer();

        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            ContractResolver = new SafeSensitiveContractResolver(),
            Error = (sender, args) => { args.ErrorContext.Handled = true; },
            Converters = { new TypeJsonConverter(), new AttributeJsonConverter() }
        };

        public (PayloadType Type, string Payload) Serialize(object source)
        {
            try
            {
                var json = JsonConvert.SerializeObject(source, Settings);
                return (PayloadType.Json, json);
            }
            catch (Exception)
            {
                return (PayloadType.Json, "{}");
            }
        }

        private class SafeSensitiveContractResolver : DefaultContractResolver
        {
            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                var property = base.CreateProperty(member, memberSerialization);

                var sensitive = member.GetCustomAttribute<SensitiveAttribute>(true);
                if (sensitive != null)
                {
                    var originalProvider = property.ValueProvider;
                    property.ValueProvider = new SensitiveValueProvider(originalProvider, property.PropertyType);
                }
                else
                {
                    var originalProvider = property.ValueProvider;
                    property.ValueProvider = new SafeValueProvider(originalProvider, property.PropertyType);
                }

                return property;
            }
        }

        private class SafeValueProvider : IValueProvider
        {
            private readonly IValueProvider _inner;
            private readonly Type _propertyType;

            public SafeValueProvider(IValueProvider inner, Type propertyType)
            {
                _inner = inner;
                _propertyType = propertyType;
            }

            public object GetValue(object target)
            {
                try
                {
                    return _inner.GetValue(target);
                }
                catch
                {
                    return _propertyType.IsValueType ? Activator.CreateInstance(_propertyType) : null;
                }
            }

            public void SetValue(object target, object value)
            {
                _inner.SetValue(target, value);
            }
        }

        private class SensitiveValueProvider : IValueProvider
        {
            private readonly IValueProvider _inner;
            private readonly Type _propertyType;

            public SensitiveValueProvider(IValueProvider inner, Type propertyType)
            {
                _inner = inner;
                _propertyType = propertyType;
            }

            public object GetValue(object target)
            {
                try
                {
                    var value = _inner.GetValue(target);

                    if (value is string s)
                    {
                        return $"Sensitive - HasValue: {!string.IsNullOrWhiteSpace(s)}";
                    }

                    return $"Sensitive - HasValue: {value != null}";
                }
                catch
                {
                    return _propertyType.IsValueType ? Activator.CreateInstance(_propertyType) : null;
                }
            }

            public void SetValue(object target, object value)
            {
                _inner.SetValue(target, value);
            }
        }

        private class TypeJsonConverter : JsonConverter<Type>
        {
            public override void WriteJson(JsonWriter writer, Type value, JsonSerializer serializer)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("Name");
                writer.WriteValue(value.GetConciseFullName());
                writer.WriteEndObject();
            }

            public override Type ReadJson(JsonReader reader, Type objectType, Type existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                throw new NotSupportedException("Type deserialization is not supported");
            }
        }

        private class AttributeJsonConverter : JsonConverter<Attribute>
        {
            public override void WriteJson(JsonWriter writer, Attribute value, JsonSerializer serializer)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("Name");
                writer.WriteValue(value.GetType().GetConciseFullName());
                writer.WriteEndObject();
            }

            public override Attribute ReadJson(JsonReader reader, Type objectType, Attribute existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                throw new NotSupportedException("Attribute deserialization is not supported");
            }
        }
    }
}
