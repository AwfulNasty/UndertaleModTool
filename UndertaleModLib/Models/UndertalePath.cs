﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UndertaleModLib.Models
{
    public class UndertalePath : UndertaleNamedResource, INotifyPropertyChanged
    {
        public UndertaleString Name { get; set; }
        public bool IsSmooth { get; set; } = false;
        public bool IsClosed { get; set; } = false;
        public uint Precision { get; set; } = 4;
        public UndertaleSimpleList<PathPoint> Points { get; private set; } = new UndertaleSimpleList<PathPoint>();

        public event PropertyChangedEventHandler PropertyChanged;

        public class PathPoint : UndertaleObject, INotifyPropertyChanged
        {
            public float X { get; set; }
            public float Y { get; set; }
            public float Speed { get; set; }

            public event PropertyChangedEventHandler PropertyChanged;

            public void Serialize(UndertaleWriter writer)
            {
                writer.Write(X);
                writer.Write(Y);
                writer.Write(Speed);
            }

            public void Unserialize(UndertaleReader reader)
            {
                X = reader.ReadSingle();
                Y = reader.ReadSingle();
                Speed = reader.ReadSingle();
            }
        }

        public void Serialize(UndertaleWriter writer)
        {
            writer.WriteUndertaleString(Name);
            writer.Write(IsSmooth);
            writer.Write(IsClosed);
            writer.Write(Precision);
            writer.WriteUndertaleObject(Points);
        }

        public void Unserialize(UndertaleReader reader)
        {
            Name = reader.ReadUndertaleString();
            IsSmooth = reader.ReadBoolean();
            IsClosed = reader.ReadBoolean();
            Precision = reader.ReadUInt32();
            Points = reader.ReadUndertaleObject<UndertaleSimpleList<PathPoint>>();
        }

        public override string ToString()
        {
            return Name.Content + " (" + GetType().Name + ")";
        }
    }
}
