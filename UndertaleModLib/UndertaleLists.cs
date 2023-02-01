﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using UndertaleModLib.Models;

namespace UndertaleModLib
{
    public abstract class UndertaleListBase<T> : ObservableCollection<T>, UndertaleObject
    {
        private readonly List<T> internalList;

        public UndertaleListBase()
        {
            try
            {
                FieldInfo itemsField = typeof(Collection<T>)
                                       .GetField("items", BindingFlags.NonPublic | BindingFlags.Instance);
                internalList = (List<T>)itemsField.GetValue(this);

            }
            catch (Exception e)
            {
                throw new UndertaleSerializationException($"{e.Message}\nwhile trying to initialize \"UndertalePointerList<{typeof(T).FullName}>\".");
            }
        }

        /// <inheritdoc />
        public abstract void Serialize(UndertaleWriter writer);

        /// <inheritdoc />
        public abstract void Unserialize(UndertaleReader reader);

        public void SetCapacity(int capacity)
        {
            try
            {
                internalList.Capacity = capacity;
            }
            catch (Exception e)
            {
                throw new UndertaleSerializationException($"{e.Message}\nwhile trying to \"SetCapacity()\" of \"UndertalePointerList<{typeof(T).FullName}>\".");
            }
        }
        public void SetCapacity(uint capacity) => SetCapacity((int)capacity);

        public void AddDirect(T item)
        {
            internalList.Add(item);
        }
    }

    public class UndertaleSimpleList<T> : UndertaleListBase<T> where T : UndertaleObject, new()
    {
        /// <inheritdoc />
        public override void Serialize(UndertaleWriter writer)
        {
            writer.Write((uint)Count);
            for (int i = 0; i < Count; i++)
            {
                try
                {
                    writer.WriteUndertaleObject<T>(this[i]);
                }
                catch (UndertaleSerializationException e)
                {
                    throw new UndertaleSerializationException(e.Message + "\nwhile writing item " + (i + 1) + " of " + Count + " in a list of " + typeof(T).FullName, e);
                }
            }
        }

        /// <inheritdoc />
        public override void Unserialize(UndertaleReader reader)
        {
            uint count = reader.ReadUInt32();
            Clear();
            SetCapacity(count);
            for (uint i = 0; i < count; i++)
            {
                try
                {
                    AddDirect(reader.ReadUndertaleObject<T>());
                }
                catch (UndertaleSerializationException e)
                {
                    throw new UndertaleSerializationException(e.Message + "\nwhile reading item " + (i + 1) + " of " + count + " in a list of " + typeof(T).FullName, e);
                }
            }
        }

        /// <inheritdoc cref="UndertaleObject.UnserializeChildObjectCount(UndertaleReader)"/>
        public static uint UnserializeChildObjectCount(UndertaleReader reader)
        {
            uint count = reader.ReadUInt32();

            Type t = typeof(T);
            if (t.IsAssignableTo(typeof(UndertaleResourceRef)))
                return count;

            if (t.IsAssignableTo(typeof(IStaticChildObjCount)))
            {
                uint subCount = reader.GetStaticChildCount(t);

                return count + count * subCount;
            }

            var unserializeFunc = reader.GetUnserializeCountFunc(t);
            for (uint i = 0; i < count; i++)
            {
                try
                {
                    count += unserializeFunc(reader);
                }
                catch (UndertaleSerializationException e)
                {
                    throw new UndertaleSerializationException(e.Message + "\nwhile reading child object count of item " + (i + 1) + " of " + count + " in a list of " + typeof(T).FullName, e);
                }
            }

            return count;
        }
    }

    public class UndertaleSimpleListString : UndertaleListBase<UndertaleString>
    {
        /// <inheritdoc />
        public override void Serialize(UndertaleWriter writer)
        {
            writer.Write((uint)Count);
            for (int i = 0; i < Count; i++)
            {
                try
                {
                    writer.WriteUndertaleString(this[i]);
                }
                catch (UndertaleSerializationException e)
                {
                    throw new UndertaleSerializationException(e.Message + "\nwhile writing item " + (i + 1) + " of " + Count + " in a string-list", e);
                }
            }
        }

        /// <inheritdoc />
        public override void Unserialize(UndertaleReader reader)
        {
            uint count = reader.ReadUInt32();
            Clear();
            SetCapacity(count);
            for (uint i = 0; i < count; i++)
            {
                try
                {
                    AddDirect(reader.ReadUndertaleString());
                }
                catch (UndertaleSerializationException e)
                {
                    throw new UndertaleSerializationException(e.Message + "\nwhile reading item " + (i + 1) + " of " + count + " in a string-list", e);
                }
            }
        }

        /// <inheritdoc cref="UndertaleObject.UnserializeChildObjectCount(UndertaleReader)"/>
        public static uint UnserializeChildObjectCount(UndertaleReader reader)
        {
            return reader.ReadUInt32();
        }
    }

    public class UndertaleSimpleListShort<T> : UndertaleListBase<T> where T : UndertaleObject, new()
    {
        private void EnsureShortCount()
        {
            if (Count > Int16.MaxValue)
                throw new InvalidOperationException("Count of short SimpleList exceeds maximum number allowed.");
        }

        /// <inheritdoc />
        public override void Serialize(UndertaleWriter writer)
        {
            writer.Write((ushort)Count);
            for (int i = 0; i < Count; i++)
            {
                try
                {
                    writer.WriteUndertaleObject<T>(this[i]);
                }
                catch (UndertaleSerializationException e)
                {
                    throw new UndertaleSerializationException(e.Message + "\nwhile writing item " + (i + 1) + " of " + Count + " in a list of " + typeof(T).FullName, e);
                }
            }
        }

        /// <inheritdoc />
        public override void Unserialize(UndertaleReader reader)
        {
            ushort count = reader.ReadUInt16();
            Clear();
            SetCapacity(count);
            for (ushort i = 0; i < count; i++)
            {
                try
                {
                    AddDirect(reader.ReadUndertaleObject<T>());
                    EnsureShortCount();
                }
                catch (UndertaleSerializationException e)
                {
                    throw new UndertaleSerializationException(e.Message + "\nwhile reading item " + (i + 1) + " of " + count + " in a list of " + typeof(T).FullName, e);
                }
            }
        }

        /// <inheritdoc cref="UndertaleObject.UnserializeChildObjectCount(UndertaleReader)"/>
        public static uint UnserializeChildObjectCount(UndertaleReader reader)
        {
            uint count = reader.ReadUInt32();

            Type t = typeof(T);
            if (t.IsAssignableTo(typeof(IStaticChildObjCount)))
            {
                uint subCount = reader.GetStaticChildCount(t);

                return count * subCount;
            }

            var unserializeFunc = reader.GetUnserializeCountFunc(t);
            for (uint i = 0; i < count; i++)
            {
                try
                {
                    count += unserializeFunc(reader);
                }
                catch (UndertaleSerializationException e)
                {
                    throw new UndertaleSerializationException(e.Message + "\nwhile reading child object count of item " + (i + 1) + " of " + count + " in a list of " + typeof(T).FullName, e);
                }
            }

            return count;
        }
    }

    public class UndertalePointerList<T> : UndertaleListBase<T> where T : UndertaleObject, new()
    {
        /// <inheritdoc />
        public override void Serialize(UndertaleWriter writer)
        {
            writer.Write((uint)Count);
            foreach (T obj in this)
                writer.WriteUndertaleObjectPointer<T>(obj);
            for (int i = 0; i < Count; i++)
            {
                try
                {
                    (this[i] as UndertaleObjectWithBlobs)?.SerializeBlobBefore(writer);
                }
                catch (UndertaleSerializationException e)
                {
                    throw new UndertaleSerializationException(e.Message + "\nwhile writing blob for item " + (i + 1) + " of " + Count + " in a list of " + typeof(T).FullName, e);
                }
            }
            for (int i = 0; i < Count; i++)
            {
                try
                {
                    T obj = this[i];

                    (obj as PrePaddedObject)?.SerializePrePadding(writer);

                    writer.WriteUndertaleObject<T>(obj);

                    // The last object does NOT get padding (TODO: at least in AUDO)
                    if (i != Count - 1)
                        (obj as PaddedObject)?.SerializePadding(writer);
                }
                catch (UndertaleSerializationException e)
                {
                    throw new UndertaleSerializationException(e.Message + "\nwhile writing item " + (i + 1) + " of " + Count + " in a list of " + typeof(T).FullName, e);
                }
            }
        }

        /// <inheritdoc />
        public override void Unserialize(UndertaleReader reader)
        {
            uint count = reader.ReadUInt32();
            Clear();
            SetCapacity(count);
            for (uint i = 0; i < count; i++)
            {
                try
                {
                    AddDirect(reader.ReadUndertaleObjectPointer<T>());
                }
                catch (UndertaleSerializationException e)
                {
                    throw new UndertaleSerializationException(e.Message + "\nwhile reading pointer to item " + (i + 1) + " of " + count + " in a list of " + typeof(T).FullName, e);
                }
            }
            if (Count > 0)
            {
                uint pos = reader.GetAddressForUndertaleObject(this[0]);
                if (reader.Position != pos)
                {
                    uint skip = pos - reader.Position;
                    if (skip > 0)
                    {
                        //Console.WriteLine("Skip " + skip + " bytes of blobs");
                        reader.Position += skip;
                    }
                    else
                        throw new IOException("First list item starts inside the pointer list?!?!");
                }
            }
            for (uint i = 0; i < count; i++)
            {
                try
                {
                    T obj = this[(int)i];

                    (obj as PrePaddedObject)?.UnserializePrePadding(reader);

                    reader.ReadUndertaleObject(obj);

                    if (i != count - 1)
                        (obj as PaddedObject)?.UnserializePadding(reader);
                }
                catch (UndertaleSerializationException e)
                {
                    throw new UndertaleSerializationException(e.Message + "\nwhile reading item " + (i + 1) + " of " + count + " in a list of " + typeof(T).FullName, e);
                }
            }
        }

        /// <inheritdoc cref="UndertaleObject.UnserializeChildObjectCount(UndertaleReader)"/>
        public static uint UnserializeChildObjectCount(UndertaleReader reader)
        {
            uint count = reader.ReadUInt32();

            Type t = typeof(T);
            if (t.IsAssignableTo(typeof(IStaticChildObjCount)))
            {
                uint subCount = reader.GetStaticChildCount(t);

                return count * subCount;
            }

            var unserializeFunc = reader.GetUnserializeCountFunc(t);
            for (uint i = 0; i < count; i++)
            {
                try
                {
                    count += unserializeFunc(reader);
                }
                catch (UndertaleSerializationException e)
                {
                    throw new UndertaleSerializationException(e.Message + "\nwhile reading child object count of item " + (i + 1) + " of " + count + " in a list of " + typeof(T).FullName, e);
                }
            }

            return count;
        }
    }

    public class UndertalePointerListLenCheck<T> : UndertalePointerList<T>, UndertaleObjectEndPos where T : UndertaleObjectLenCheck, new()
    {
        /// <inheritdoc cref="UndertalePointerList{T}.Unserialize(UndertaleReader)" />
        public void Unserialize(UndertaleReader reader, uint endPosition)
        {
            uint count = reader.ReadUInt32();
            Clear();
            SetCapacity(count);
            List<uint> pointers = new((int)count);
            for (uint i = 0; i < count; i++)
            {
                try
                {
                    uint ptr = reader.ReadUInt32();
                    pointers.Add(ptr);
                    AddDirect(reader.GetUndertaleObjectAtAddress<T>(ptr));
                }
                catch (UndertaleSerializationException e)
                {
                    throw new UndertaleSerializationException(e.Message + "\nwhile reading pointer to item " + (i + 1) + " of " + count + " in a list of " + typeof(T).FullName, e);
                }
            }
            if (Count > 0)
            {
                uint pos = reader.GetAddressForUndertaleObject(this[0]);
                if (reader.Position != pos)
                {
                    uint skip = pos - reader.Position;
                    if (skip > 0)
                    {
                        //Console.WriteLine("Skip " + skip + " bytes of blobs");
                        reader.Position += skip;
                    }
                    else
                        throw new IOException("First list item starts inside the pointer list?!?!");
                }
            }
            for (uint i = 0; i < count; i++)
            {
                try
                {
                    T obj = this[(int)i];

                    (obj as PrePaddedObject)?.UnserializePrePadding(reader);
                    if ((i + 1) < count)
                        reader.ReadUndertaleObject(obj, (int)(pointers[(int)i + 1] - reader.Position));
                    else
                        reader.ReadUndertaleObject(obj, (int)(endPosition - reader.Position));
                    if (i != count - 1)
                        (obj as PaddedObject)?.UnserializePadding(reader);
                }
                catch (UndertaleSerializationException e)
                {
                    throw new UndertaleSerializationException(e.Message + "\nwhile reading item " + (i + 1) + " of " + count + " in a list of " + typeof(T).FullName, e);
                }
            }
        }
    }

    public class UndertaleSimpleResourcesList<T, ChunkT> : UndertaleSimpleList<UndertaleResourceById<T, ChunkT>> where T : UndertaleResource, new() where ChunkT : UndertaleListChunk<T>
    {
        // TODO: Allow direct access to Resource elements?
    }
}
