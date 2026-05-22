using System.Buffers.Binary;
using System.Text;
using System.Text.Json;
using MokoSnap.Core.Storage;

namespace MokoSnap.Core.NativeMessaging;

public static class NativeMessageProtocol
{
    public static async Task<T?> ReadMessageAsync<T>(Stream input, CancellationToken cancellationToken = default)
    {
        byte[] lengthBuffer = new byte[4];
        int lengthBytesRead = await ReadExactOrEndAsync(input, lengthBuffer, cancellationToken);
        if (lengthBytesRead == 0)
        {
            return default;
        }

        if (lengthBytesRead != lengthBuffer.Length)
        {
            throw new EndOfStreamException("Native Messaging length prefix was incomplete.");
        }

        int length = BinaryPrimitives.ReadInt32LittleEndian(lengthBuffer);
        if (length < 0)
        {
            throw new InvalidDataException("Native Messaging message length was invalid.");
        }

        byte[] payload = new byte[length];
        int payloadBytesRead = await ReadExactOrEndAsync(input, payload, cancellationToken);
        if (payloadBytesRead != length)
        {
            throw new EndOfStreamException("Native Messaging payload was incomplete.");
        }

        return JsonSerializer.Deserialize<T>(
            Encoding.UTF8.GetString(payload),
            FileJsonStorage<T>.CreateJsonSerializerOptions());
    }

    public static async Task WriteMessageAsync<T>(
        Stream output,
        T message,
        CancellationToken cancellationToken = default)
    {
        byte[] payload = JsonSerializer.SerializeToUtf8Bytes(
            message,
            FileJsonStorage<T>.CreateJsonSerializerOptions());
        byte[] lengthBuffer = new byte[4];
        BinaryPrimitives.WriteInt32LittleEndian(lengthBuffer, payload.Length);
        await output.WriteAsync(lengthBuffer, cancellationToken);
        await output.WriteAsync(payload, cancellationToken);
        await output.FlushAsync(cancellationToken);
    }

    private static async Task<int> ReadExactOrEndAsync(
        Stream input,
        byte[] buffer,
        CancellationToken cancellationToken)
    {
        int offset = 0;
        while (offset < buffer.Length)
        {
            int read = await input.ReadAsync(buffer.AsMemory(offset, buffer.Length - offset), cancellationToken);
            if (read == 0)
            {
                return offset;
            }

            offset += read;
        }

        return offset;
    }
}
