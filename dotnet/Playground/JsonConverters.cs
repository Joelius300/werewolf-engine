using System.Text.Json;
using System.Text.Json.Serialization;
using WerewolfEngine.Actions;
using WerewolfEngine.Rules;

namespace Playground;

internal class JsonTypeConverter : JsonConverter<Type>
{
    public override Type Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        throw new NotImplementedException();

    public override void Write(Utf8JsonWriter writer, Type value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.FullName);
}

internal class JsonActualTypeConverter<T> : JsonConverter<T>
    where T : class
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        throw new NotImplementedException();

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) =>
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
}

internal class JsonGenericActionConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        if (!typeToConvert.IsGenericType)
            return false;

        return typeToConvert.GetGenericTypeDefinition() == typeof(IAction<,>);
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options) =>
        (JsonConverter?) Activator.CreateInstance(
            typeof(JsonGenericActionConverter<,>).MakeGenericType(typeToConvert.GetGenericArguments()));
}

internal class JsonGenericActionConverter<TInputRequest, TInputResponse> : JsonActualTypeConverter<
    IAction<TInputRequest, TInputResponse>>
    where TInputRequest : IInputRequest
    where TInputResponse : IInputResponse
{
}

internal class JsonTagConverter : JsonConverter<Tag>
{
    public override Tag? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotImplementedException();

    public override void Write(Utf8JsonWriter writer, Tag value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Identifier);
    }
}