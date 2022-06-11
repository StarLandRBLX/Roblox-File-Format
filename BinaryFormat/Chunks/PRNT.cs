﻿using System;
using System.Collections.Generic;
using System.Text;

namespace RobloxFiles.BinaryFormat.Chunks
{
    public class PRNT : IBinaryFileChunk
    {
        private const byte FORMAT = 0;
        private BinaryRobloxFile File;

        public void Load(BinaryRobloxFileReader reader)
        {
            BinaryRobloxFile file = reader.File;
            File = file;

            byte format = reader.ReadByte();
            int idCount = reader.ReadInt32();

            if (format != FORMAT)
                throw new Exception($"Unexpected PRNT format: {format} (expected {FORMAT}!)");

            var childIds = reader.ReadInstanceIds(idCount);
            var parentIds = reader.ReadInstanceIds(idCount);
            
            for (int i = 0; i < idCount; i++)
            {
                int childId = childIds[i];
                int parentId = parentIds[i];

                Instance child = file.Instances[childId];
                Instance parent = (parentId >= 0 ? file.Instances[parentId] : file);

                if (child == null)
                {
                    if (RobloxFile.LogErrors)
                        Console.Error.WriteLine($"PRNT: could not parent {childId} to {parentId} because child {childId} was null.");

                    continue;
                }

                if (parentId >= 0 && parent == null)
                {
                    if (RobloxFile.LogErrors)
                        Console.Error.WriteLine($"PRNT: could not parent {childId} to {parentId} because parent {parentId} was null.");

                    continue;
                }

                child.Parent = (parentId >= 0 ? file.Instances[parentId] : file);
            }
        }

        public void Save(BinaryRobloxFileWriter writer)
        {
            var file = writer.File;
            File = file;

            var postInstances = writer.PostInstances;
            var idCount = postInstances.Count;

            var childIds = new List<int>();
            var parentIds = new List<int>();

            foreach (Instance inst in writer.PostInstances)
            {
                Instance parent = inst.Parent;

                int childId = int.Parse(inst.Referent);
                int parentId = -1;

                if (parent != null)
                    parentId = int.Parse(parent.Referent);

                childIds.Add(childId);
                parentIds.Add(parentId);
            }

            writer.Write(FORMAT);
            writer.Write(idCount);

            writer.WriteInstanceIds(childIds);
            writer.WriteInstanceIds(parentIds);
        }

        public void WriteInfo(StringBuilder builder)
        {
            var childIds = new List<int>();
            var parentIds = new List<int>();

            foreach (Instance inst in File.GetDescendants())
            {
                Instance parent = inst.Parent;

                int childId = int.Parse(inst.Referent);
                int parentId = -1;

                if (parent != null)
                    parentId = int.Parse(parent.Referent);

                childIds.Add(childId);
                parentIds.Add(parentId);
            }

            builder.AppendLine($"- Format:    {FORMAT}");
            builder.AppendLine($"- ChildIds:  {string.Join(", ", childIds)}");
            builder.AppendLine($"- ParentIds: {string.Join(", ", parentIds)}");
        }
    }
}
