// <auto-generated>
//  automatically generated by the FlatBuffers compiler, do not modify
// </auto-generated>

namespace Revit_Transform.Flat.Schema
{

    using global::System;
    using global::System.Collections.Generic;
    using global::Google.FlatBuffers;

    public struct TextureBuffer : IFlatbufferObject
    {
        private Table __p;
        public ByteBuffer ByteBuffer { get { return __p.bb; } }
        public static void ValidateVersion() { FlatBufferConstants.FLATBUFFERS_23_5_26(); }
        public static TextureBuffer GetRootAsTextureBuffer(ByteBuffer _bb) { return GetRootAsTextureBuffer(_bb, new TextureBuffer()); }
        public static TextureBuffer GetRootAsTextureBuffer(ByteBuffer _bb, TextureBuffer obj) { return (obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
        public void __init(int _i, ByteBuffer _bb) { __p = new Table(_i, _bb); }
        public TextureBuffer __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }

        public string Image { get { int o = __p.__offset(4); return o != 0 ? __p.__string(o + __p.bb_pos) : null; } }
#if ENABLE_SPAN_T
  public Span<byte> GetImageBytes() { return __p.__vector_as_span<byte>(4, 1); }
#else
        public ArraySegment<byte>? GetImageBytes() { return __p.__vector_as_arraysegment(4); }
#endif
        public byte[] GetImageArray() { return __p.__vector_as_array<byte>(4); }
        public int Repeat(int j) { int o = __p.__offset(6); return o != 0 ? __p.bb.GetInt(__p.__vector(o) + j * 4) : (int)0; }
        public int RepeatLength { get { int o = __p.__offset(6); return o != 0 ? __p.__vector_len(o) : 0; } }
#if ENABLE_SPAN_T
  public Span<int> GetRepeatBytes() { return __p.__vector_as_span<int>(6, 4); }
#else
        public ArraySegment<byte>? GetRepeatBytes() { return __p.__vector_as_arraysegment(6); }
#endif
        public int[] GetRepeatArray() { return __p.__vector_as_array<int>(6); }
        public int Wrap(int j) { int o = __p.__offset(8); return o != 0 ? __p.bb.GetInt(__p.__vector(o) + j * 4) : (int)0; }
        public int WrapLength { get { int o = __p.__offset(8); return o != 0 ? __p.__vector_len(o) : 0; } }
#if ENABLE_SPAN_T
  public Span<int> GetWrapBytes() { return __p.__vector_as_span<int>(8, 4); }
#else
        public ArraySegment<byte>? GetWrapBytes() { return __p.__vector_as_arraysegment(8); }
#endif
        public int[] GetWrapArray() { return __p.__vector_as_array<int>(8); }
        public string Uuid { get { int o = __p.__offset(10); return o != 0 ? __p.__string(o + __p.bb_pos) : null; } }
#if ENABLE_SPAN_T
  public Span<byte> GetUuidBytes() { return __p.__vector_as_span<byte>(10, 1); }
#else
        public ArraySegment<byte>? GetUuidBytes() { return __p.__vector_as_arraysegment(10); }
#endif
        public byte[] GetUuidArray() { return __p.__vector_as_array<byte>(10); }

        public static Offset<Schema.TextureBuffer> CreateTextureBuffer(FlatBufferBuilder builder,
            StringOffset imageOffset = default(StringOffset),
            VectorOffset repeatOffset = default(VectorOffset),
            VectorOffset wrapOffset = default(VectorOffset),
            StringOffset uuidOffset = default(StringOffset))
        {
            builder.StartTable(4);
            TextureBuffer.AddUuid(builder, uuidOffset);
            TextureBuffer.AddWrap(builder, wrapOffset);
            TextureBuffer.AddRepeat(builder, repeatOffset);
            TextureBuffer.AddImage(builder, imageOffset);
            return TextureBuffer.EndTextureBuffer(builder);
        }

        public static void StartTextureBuffer(FlatBufferBuilder builder) { builder.StartTable(4); }
        public static void AddImage(FlatBufferBuilder builder, StringOffset imageOffset) { builder.AddOffset(0, imageOffset.Value, 0); }
        public static void AddRepeat(FlatBufferBuilder builder, VectorOffset repeatOffset) { builder.AddOffset(1, repeatOffset.Value, 0); }
        public static VectorOffset CreateRepeatVector(FlatBufferBuilder builder, int[] data) { builder.StartVector(4, data.Length, 4); for (int i = data.Length - 1; i >= 0; i--) builder.AddInt(data[i]); return builder.EndVector(); }
        public static VectorOffset CreateRepeatVectorBlock(FlatBufferBuilder builder, int[] data) { builder.StartVector(4, data.Length, 4); builder.Add(data); return builder.EndVector(); }
        public static VectorOffset CreateRepeatVectorBlock(FlatBufferBuilder builder, ArraySegment<int> data) { builder.StartVector(4, data.Count, 4); builder.Add(data); return builder.EndVector(); }
        public static VectorOffset CreateRepeatVectorBlock(FlatBufferBuilder builder, IntPtr dataPtr, int sizeInBytes) { builder.StartVector(1, sizeInBytes, 1); builder.Add<int>(dataPtr, sizeInBytes); return builder.EndVector(); }
        public static void StartRepeatVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(4, numElems, 4); }
        public static void AddWrap(FlatBufferBuilder builder, VectorOffset wrapOffset) { builder.AddOffset(2, wrapOffset.Value, 0); }
        public static VectorOffset CreateWrapVector(FlatBufferBuilder builder, int[] data) { builder.StartVector(4, data.Length, 4); for (int i = data.Length - 1; i >= 0; i--) builder.AddInt(data[i]); return builder.EndVector(); }
        public static VectorOffset CreateWrapVectorBlock(FlatBufferBuilder builder, int[] data) { builder.StartVector(4, data.Length, 4); builder.Add(data); return builder.EndVector(); }
        public static VectorOffset CreateWrapVectorBlock(FlatBufferBuilder builder, ArraySegment<int> data) { builder.StartVector(4, data.Count, 4); builder.Add(data); return builder.EndVector(); }
        public static VectorOffset CreateWrapVectorBlock(FlatBufferBuilder builder, IntPtr dataPtr, int sizeInBytes) { builder.StartVector(1, sizeInBytes, 1); builder.Add<int>(dataPtr, sizeInBytes); return builder.EndVector(); }
        public static void StartWrapVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(4, numElems, 4); }
        public static void AddUuid(FlatBufferBuilder builder, StringOffset uuidOffset) { builder.AddOffset(3, uuidOffset.Value, 0); }
        public static Offset<Schema.TextureBuffer> EndTextureBuffer(FlatBufferBuilder builder)
        {
            int o = builder.EndTable();
            return new Offset<Schema.TextureBuffer>(o);
        }
    }


    static public class TextureBufferVerify
    {
        static public bool Verify(Google.FlatBuffers.Verifier verifier, uint tablePos)
        {
            return verifier.VerifyTableStart(tablePos)
              && verifier.VerifyString(tablePos, 4 /*Image*/, false)
              && verifier.VerifyVectorOfData(tablePos, 6 /*Repeat*/, 4 /*int*/, false)
              && verifier.VerifyVectorOfData(tablePos, 8 /*Wrap*/, 4 /*int*/, false)
              && verifier.VerifyString(tablePos, 10 /*Uuid*/, false)
              && verifier.VerifyTableEnd(tablePos);
        }
    }

}
