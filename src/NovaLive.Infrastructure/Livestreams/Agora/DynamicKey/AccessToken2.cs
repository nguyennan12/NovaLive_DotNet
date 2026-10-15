using System.Text;

namespace NovaLive.Infrastructure.Livestreams.Agora.DynamicKey;

public class AccessToken2
{
    public const string Version = "007";
    public const int VersionLength = 3;

    public const short ServiceTypeRtc = 1;
    public const short ServiceTypeRtm = 2;

    public string AppCert { get; set; } = string.Empty;
    public string AppId { get; set; } = string.Empty;
    public uint Expire { get; set; }
    public uint IssueTs { get; set; }
    public uint Salt { get; set; }
    public List<Service> Services { get; } = new();

    private byte[] _signature = Array.Empty<byte>();
    private byte[] _signingInfo = Array.Empty<byte>();

    public AccessToken2()
    {
    }

    public AccessToken2(string appId, string appCert, uint expire, uint issueTs, uint salt)
    {
        AppId = appId;
        AppCert = appCert;
        Expire = expire;
        IssueTs = issueTs;
        Salt = salt;
    }

    public void AddService(Service service)
    {
        Services.Add(service);
    }

    public List<Service> GetServices(short serviceType)
    {
        return Services.Where(s => s.GetServiceType() == serviceType).ToList();
    }

    public byte[] GetSign(string appCertificate)
    {
        byte[] signing = DynamicKeyUtil.EncodeHmacSha256(BitConverter.GetBytes(IssueTs), Encoding.UTF8.GetBytes(appCertificate));
        return DynamicKeyUtil.EncodeHmacSha256(BitConverter.GetBytes(Salt), signing);
    }

    public string Build()
    {
        if (!DynamicKeyUtil.IsHex32(AppId) || !DynamicKeyUtil.IsHex32(AppCert) || Services.Count == 0)
        {
            return string.Empty;
        }

        List<Service> servicesForPacking = Services.OrderBy(s => s.GetServiceType()).ToList();

        var buf = new ByteBuf()
            .Put(Encoding.UTF8.GetBytes(AppId))
            .Put(IssueTs)
            .Put(Expire)
            .Put(Salt)
            .Put((ushort)servicesForPacking.Count);

        byte[] signing = GetSign(AppCert);

        foreach (Service service in servicesForPacking)
        {
            service.Pack(buf);
        }

        byte[] signature = DynamicKeyUtil.EncodeHmacSha256(signing, buf.AsBytes());

        var bufferContent = new ByteBuf();
        bufferContent.Put(signature);
        bufferContent.Copy(buf.AsBytes());

        return Version + Convert.ToBase64String(DynamicKeyUtil.Compress(bufferContent.AsBytes()));
    }

    public bool Parse(string? token)
    {
        AppId = string.Empty;
        IssueTs = 0;
        Expire = 0;
        Salt = 0;
        Services.Clear();
        _signature = Array.Empty<byte>();
        _signingInfo = Array.Empty<byte>();

        if (string.IsNullOrWhiteSpace(token) || token.Length < VersionLength ||
            !token.StartsWith(Version, StringComparison.Ordinal))
        {
            return false;
        }

        try
        {
            byte[] data = DynamicKeyUtil.Decompress(Convert.FromBase64String(token[VersionLength..]));

            var buff = new ByteBuf(data);
            _signature = buff.ReadBytes();
            _signingInfo = data.Skip(sizeof(ushort) + _signature.Length).ToArray();
            Services.Clear();

            AppId = Encoding.UTF8.GetString(buff.ReadBytes());
            IssueTs = buff.ReadInt();
            Expire = buff.ReadInt();
            Salt = buff.ReadInt();
            ushort servicesNum = buff.ReadShort();

            for (ushort i = 0; i < servicesNum; i++)
            {
                short serviceType = (short)buff.ReadShort();
                Service? service = CreateService(serviceType);
                if (service is null)
                {
                    return false;
                }
                service.Unpack(buff);
                Services.Add(service);
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool VerifySignature(string? appCertificate)
    {
        if (string.IsNullOrEmpty(appCertificate) || _signature.Length == 0 || _signingInfo.Length == 0)
        {
            return false;
        }

        try
        {
            byte[] signing = GetSign(appCertificate);
            byte[] expectedSignature = DynamicKeyUtil.EncodeHmacSha256(signing, _signingInfo);
            return _signature.SequenceEqual(expectedSignature);
        }
        catch
        {
            return false;
        }
    }

    private static Service? CreateService(short serviceType)
    {
        return serviceType switch
        {
            ServiceTypeRtc => new ServiceRtc(),
            ServiceTypeRtm => new ServiceRtm(),
            _ => null
        };
    }

    public enum PrivilegeRtcEnum : ushort
    {
        PrivilegeJoinChannel = 1,
        PrivilegePublishAudioStream = 2,
        PrivilegePublishVideoStream = 3,
        PrivilegePublishDataStream = 4
    }

    public enum PrivilegeRtmEnum : ushort
    {
        PrivilegeLogin = 1
    }

    public class Service
    {
        private short _type;
        private readonly SortedDictionary<ushort, uint> _privileges = new();

        public Service()
        {
        }

        public Service(short serviceType)
        {
            _type = serviceType;
        }

        public short GetServiceType() => _type;
        public void SetServiceType(short serviceType) => _type = serviceType;

        public SortedDictionary<ushort, uint> GetPrivileges() => _privileges;

        public void AddPrivilegeRtc(PrivilegeRtcEnum privilege, uint expire)
        {
            _privileges[(ushort)privilege] = expire;
        }

        public void AddPrivilegeRtm(PrivilegeRtmEnum privilege, uint expire)
        {
            _privileges[(ushort)privilege] = expire;
        }

        public virtual ByteBuf Pack(ByteBuf buf)
        {
            return buf.Put((ushort)_type).PutIntMap(_privileges);
        }

        public virtual void Unpack(ByteBuf byteBuf)
        {
            var map = byteBuf.ReadIntMap();
            foreach (var kv in map)
            {
                _privileges[kv.Key] = kv.Value;
            }
        }
    }

    public class ServiceRtc : Service
    {
        public string ChannelName { get; set; } = string.Empty;
        public string Uid { get; set; } = string.Empty;

        public ServiceRtc()
        {
            SetServiceType(ServiceTypeRtc);
        }

        public ServiceRtc(string channelName, string uid)
        {
            SetServiceType(ServiceTypeRtc);
            ChannelName = channelName;
            Uid = uid;
        }

        public override ByteBuf Pack(ByteBuf buf)
        {
            return base.Pack(buf)
                .Put(Encoding.UTF8.GetBytes(ChannelName))
                .Put(Encoding.UTF8.GetBytes(Uid));
        }

        public override void Unpack(ByteBuf byteBuf)
        {
            base.Unpack(byteBuf);
            ChannelName = Encoding.UTF8.GetString(byteBuf.ReadBytes());
            Uid = Encoding.UTF8.GetString(byteBuf.ReadBytes());
        }
    }

    public class ServiceRtm : Service
    {
        public string UserId { get; set; } = string.Empty;

        public ServiceRtm()
        {
            SetServiceType(ServiceTypeRtm);
        }

        public ServiceRtm(string userId)
        {
            SetServiceType(ServiceTypeRtm);
            UserId = userId;
        }

        public override ByteBuf Pack(ByteBuf buf)
        {
            return base.Pack(buf).Put(Encoding.UTF8.GetBytes(UserId));
        }

        public override void Unpack(ByteBuf byteBuf)
        {
            base.Unpack(byteBuf);
            UserId = Encoding.UTF8.GetString(byteBuf.ReadBytes());
        }
    }
}
