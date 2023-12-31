// <auto-generated>
//  automatically generated by the FlatBuffers compiler, do not modify
// </auto-generated>

namespace Revit_Transform.Flat.Schema
{

    using global::System;
    using global::System.Collections.Generic;
    using global::Google.FlatBuffers;

    public struct ImageBuffer : IFlatbufferObject
    {
        private Table __p;
        public ByteBuffer ByteBuffer { get { return __p.bb; } }
        public static void ValidateVersion() { FlatBufferConstants.FLATBUFFERS_23_5_26(); }
        public static ImageBuffer GetRootAsImageBuffer(ByteBuffer _bb) { return GetRootAsImageBuffer(_bb, new ImageBuffer()); }
        public static ImageBuffer GetRootAsImageBuffer(ByteBuffer _bb, ImageBuffer obj) { return (obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
        public void __init(int _i, ByteBuffer _bb) { __p = new Table(_i, _bb); }
        public ImageBuffer __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }

        public string Uuid { get { int o = __p.__offset(4); return o != 0 ? __p.__string(o + __p.bb_pos) : null; } }
#if ENABLE_SPAN_T
  public Span<byte> GetUuidBytes() { return __p.__vector_as_span<byte>(4, 1); }
#else
        public ArraySegment<byte>? GetUuidBytes() { return __p.__vector_as_arraysegment(4); }
#endif
        public byte[] GetUuidArray() { return __p.__vector_as_array<byte>(4); }
        public string Url { get { int o = __p.__offset(6); return o != 0 ? __p.__string(o + __p.bb_pos) : null; } }
#if ENABLE_SPAN_T
  public Span<byte> GetUrlBytes() { return __p.__vector_as_span<byte>(6, 1); }
#else
        public ArraySegment<byte>? GetUrlBytes() { return __p.__vector_as_arraysegment(6); }
#endif
        public byte[] GetUrlArray() { return __p.__vector_as_array<byte>(6); }

        public static Offset<Schema.ImageBuffer> CreateImageBuffer(FlatBufferBuilder builder,
            StringOffset uuidOffset = default(StringOffset),
            StringOffset urlOffset = default(StringOffset))
        {
            builder.StartTable(2);
            ImageBuffer.AddUrl(builder, urlOffset);
            ImageBuffer.AddUuid(builder, uuidOffset);
            return ImageBuffer.EndImageBuffer(builder);
        }

        public static void StartImageBuffer(FlatBufferBuilder builder) { builder.StartTable(2); }
        public static void AddUuid(FlatBufferBuilder builder, StringOffset uuidOffset) { builder.AddOffset(0, uuidOffset.Value, 0); }
        public static void AddUrl(FlatBufferBuilder builder, StringOffset urlOffset) { builder.AddOffset(1, urlOffset.Value, 0); }
        public static Offset<Schema.ImageBuffer> EndImageBuffer(FlatBufferBuilder builder)
        {
            int o = builder.EndTable();
            return new Offset<Schema.ImageBuffer>(o);
        }
    }


    static public class ImageBufferVerify
    {
        static public bool Verify(Google.FlatBuffers.Verifier verifier, uint tablePos)
        {
            return verifier.VerifyTableStart(tablePos)
              && verifier.VerifyString(tablePos, 4 /*Uuid*/, false)
              && verifier.VerifyString(tablePos, 6 /*Url*/, false)
              && verifier.VerifyTableEnd(tablePos);
        }
    }

}
