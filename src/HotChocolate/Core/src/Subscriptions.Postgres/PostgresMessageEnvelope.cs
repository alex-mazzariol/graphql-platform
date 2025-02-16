using System.Buffers;
using System.Buffers.Text;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace HotChocolate.Subscriptions.Postgres;

internal readonly struct PostgresMessageEnvelope
{
    private static readonly Encoding _utf8 = Encoding.UTF8;
    private static readonly Random _random = Random.Shared;
    private const byte separator = (byte)':';
    private const byte _messageIdLength = 24;

    public PostgresMessageEnvelope(string topic, string payload)
    {
        Topic = topic;
        Payload = payload;
    }

    public string Topic { get; }

    public string Payload { get; }

    public string Format()
    {
        var topicMaxBytesCount = _utf8.GetMaxByteCount(Topic.Length);
        var payloadMaxBytesCount = _utf8.GetMaxByteCount(Payload.Length);
        // we encode the topic to base64 to ensure that we do not have the separator in the topic
        var topicMaxLength = Base64.GetMaxEncodedToUtf8Length(topicMaxBytesCount);
        var maxSize = topicMaxLength + 2 + payloadMaxBytesCount + _messageIdLength;

        byte[]? bufferArray = null;
        var buffer = maxSize < 1024
            ? stackalloc byte[maxSize]
            : bufferArray = ArrayPool<byte>.Shared.Rent(maxSize);

        var slicedBuffer = buffer;

        // prefix with id
        var id = buffer[.._messageIdLength];
        _random.NextBytes(id);

        const int numberOfLetter = 26;
        const int asciiCodeOfLowerCaseA = 97;
        for (var i = 0; i < _messageIdLength; i++)
        {
            slicedBuffer[i] = (byte)(id[i] % numberOfLetter + asciiCodeOfLowerCaseA);
        }

        slicedBuffer = slicedBuffer[_messageIdLength..];

        // write separator
        slicedBuffer[0] = separator;
        slicedBuffer = slicedBuffer[1..];

        // write topic as base64
        var topicLengthUtf8 = _utf8.GetBytes(Topic, slicedBuffer);
        Base64.EncodeToUtf8InPlace(slicedBuffer, topicLengthUtf8, out var topicLengthBase64);
        slicedBuffer = slicedBuffer[topicLengthBase64..];

        // write separator
        slicedBuffer[0] = separator;
        slicedBuffer = slicedBuffer[1..];

        // write payload
        var payloadLengthUtf8 = _utf8.GetBytes(Payload, slicedBuffer);

        // create string
        var endOfEncodedString = topicLengthBase64 + 2 + payloadLengthUtf8 + _messageIdLength;
        var result = _utf8.GetString(buffer[..endOfEncodedString]);

        if (bufferArray is not null)
        {
            ArrayPool<byte>.Shared.Return(bufferArray);
        }

        return result;
    }

    public static bool TryParse(
        string message,
        [NotNullWhen(true)] out string? topic,
        [NotNullWhen(true)] out string? payload)
    {
        var maxSize = _utf8.GetMaxByteCount(message.Length);

        byte[]? bufferArray = null;
        var buffer = maxSize < 1024
            ? stackalloc byte[maxSize]
            : bufferArray = ArrayPool<byte>.Shared.Rent(maxSize);

        // get the bytes of the message
        var utf8ByteLength = _utf8.GetBytes(message, buffer);

        // slice the buffer to the actual length
        buffer = buffer[..utf8ByteLength];

        // remove message id and separator
        buffer = buffer[(_messageIdLength + 1)..];

        // find the separator
        var indexOfColon = buffer.IndexOf(separator);
        if (indexOfColon == -1)
        {
            topic = null;
            payload = null;
            return false;
        }

        var topicLengthBase64 = indexOfColon;
        Base64.DecodeFromUtf8InPlace(buffer[..topicLengthBase64], out var topicLengthUtf8);

        topic = _utf8.GetString(buffer[..topicLengthUtf8]);
        payload = _utf8.GetString(buffer[(indexOfColon + 1)..]);

        if (bufferArray is not null)
        {
            ArrayPool<byte>.Shared.Return(bufferArray);
        }

        return true;
    }
}
