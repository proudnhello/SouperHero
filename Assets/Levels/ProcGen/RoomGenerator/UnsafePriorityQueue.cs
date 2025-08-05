using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

// inspo from https://medium.com/@basilin/priority-queue-with-c-7089f4898c8d
[BurstCompatible]
public unsafe struct UnsafePriorityQueue
{
    [BurstCompile]
    [StructLayout(LayoutKind.Sequential)]
    public struct Entry
    {
        public Coord coord;
        public float priority;

        public Entry(Coord coord, float priority)
        {
            this.coord = coord;
            this.priority = priority;
        }
    }
    public UnsafeList<Entry> Entries;
    public int Count
    {
        get => Entries.Length;
    }

    public Coord Dequeue()
    {
        if (!Entries.IsEmpty)
        {
            var itemToBeRemoved = Entries[0];
            Entries.RemoveAt(0);
            return itemToBeRemoved.coord;
        }
        return new Coord(-1, -1);
    }

    public void Enqueue(Coord coords, float priority)
    {
        if (Entries.IsEmpty)
        {
            //Debug.Log("1 Enqueuing (" + coords.x + "," + coords.y + ") - prio = " + priority);
            Entries.Add(new(coords, priority));
            return;
        }

        for (int i = 0; i < Entries.Length; i++)
        {

            if (priority < Entries[i].priority)
            {
                if (i == 0)
                {
                    //Debug.Log("3 Enqueuing (" + coords.x + "," + coords.y + ") - prio = " + Entries[i].priority + " -> " + priority);
                    UnsafeList<Entry> newList = new(0, Allocator.Persistent)
                    {
                        new(coords, priority)
                    };
                    newList.AddRange(Entries);
                    Entries.Dispose();
                    Entries = newList;
                    return;
                }
                else
                {
                    //Debug.Log("4 Enqueuing (" + coords.x + "," + coords.y + ") - prio = " + Entries[i].priority + " -> " + priority);
                    UnsafeList<Entry> newList = new(0, Allocator.Persistent);
                    newList.CopyFrom(Entries);
                    newList.RemoveRange(i, Entries.Length-i);
                    newList.Add(new(coords, priority));
                    Entries.RemoveRange(0, i);
                    newList.AddRange(Entries);
                    Entries.Dispose();
                    Entries = newList;
                    return;
                }
            }
            else if (i == Entries.Length - 1)
            {
                Entries.Add(new(coords, priority));
                return;
            }
        }
    }
}