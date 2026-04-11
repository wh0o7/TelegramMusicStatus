using System.Buffers.Binary;
using System.Net.Sockets;
using System.Text;

namespace TelegramMusicStatus.Net;

internal static class Socks5TcpFactory
{
    public static TcpClient CreateConnectedClient(
        string targetHost,
        int targetPort,
        string proxyHost,
        int proxyPort,
        string? username,
        string? password)
    {
        var proxy = new TcpClient();
        proxy.Connect(proxyHost, proxyPort);
        var stream = proxy.GetStream();
        var buf = new byte[512];

        if (string.IsNullOrEmpty(username))
            stream.Write([5, 1, 0]);
        else
            stream.Write([5, 1, 2]);

        ReadExact(stream, buf.AsSpan(0, 2));
        if (buf[0] != 5)
            throw new IOException("SOCKS5: invalid greeting response");

        switch (buf[1])
        {
            case 255:
                throw new IOException("SOCKS5: no acceptable authentication method");
            case 2:
                if (string.IsNullOrEmpty(username))
                    throw new IOException("SOCKS5: proxy requires credentials");
                SendUserPassAuth(stream, username!, password ?? string.Empty);
                break;
            case 0:
                break;
            default:
                throw new IOException($"SOCKS5: unsupported method {buf[1]}");
        }

        var hostBytes = Encoding.UTF8.GetBytes(targetHost);
        if (hostBytes.Length > 255)
            throw new ArgumentException("Target hostname is too long for SOCKS5.", nameof(targetHost));

        using var req = new MemoryStream(6 + hostBytes.Length);
        req.WriteByte(5);
        req.WriteByte(1);
        req.WriteByte(0);
        req.WriteByte(3);
        req.WriteByte((byte)hostBytes.Length);
        req.Write(hostBytes);
        Span<byte> portSpan = stackalloc byte[2];
        BinaryPrimitives.WriteUInt16BigEndian(portSpan, (ushort)targetPort);
        req.Write(portSpan);
        stream.Write(req.ToArray());

        ReadExact(stream, buf.AsSpan(0, 4));
        if (buf[0] != 5)
            throw new IOException("SOCKS5: invalid reply version");
        if (buf[1] != 0)
            throw new IOException($"SOCKS5: connect failed, reply code {buf[1]}");

        SkipBindAddress(stream, buf, buf[3]);
        return proxy;
    }

    private static void SkipBindAddress(NetworkStream stream, byte[] buf, byte atyp)
    {
        switch (atyp)
        {
            case 1:
                ReadExact(stream, buf.AsSpan(0, 4 + 2));
                break;
            case 3:
                ReadExact(stream, buf.AsSpan(0, 1));
                var len = buf[0];
                ReadExact(stream, buf.AsSpan(0, len + 2));
                break;
            case 4:
                ReadExact(stream, buf.AsSpan(0, 16 + 2));
                break;
            default:
                throw new IOException($"SOCKS5: unknown address type {atyp}");
        }
    }

    private static void SendUserPassAuth(NetworkStream stream, string user, string pass)
    {
        var ub = Encoding.UTF8.GetBytes(user);
        var pb = Encoding.UTF8.GetBytes(pass);
        if (ub.Length > 255 || pb.Length > 255)
            throw new ArgumentException("SOCKS5 username or password is too long.");

        using var ms = new MemoryStream(3 + ub.Length + pb.Length);
        ms.WriteByte(1);
        ms.WriteByte((byte)ub.Length);
        ms.Write(ub);
        ms.WriteByte((byte)pb.Length);
        ms.Write(pb);
        stream.Write(ms.ToArray());

        Span<byte> authReply = stackalloc byte[2];
        ReadExact(stream, authReply);
        if (authReply[0] != 1 || authReply[1] != 0)
            throw new IOException("SOCKS5: username/password authentication failed");
    }

    private static void ReadExact(NetworkStream stream, Span<byte> buffer)
    {
        var read = 0;
        while (read < buffer.Length)
        {
            var n = stream.Read(buffer[read..]);
            if (n == 0)
                throw new IOException("SOCKS5: connection closed unexpectedly");
            read += n;
        }
    }
}