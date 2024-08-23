using HarmonyLib;
using ProtoBuf;
using Snitches.BlockEntities;
using Snitches.Violation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Util;

namespace Snitches.Violations
{
	[ProtoContract]
	public class SerializationCallback
	{
		public delegate void OnSerializationDelegate(IServerChunk chunk);

		readonly private IServerChunk _chunk;
		public OnSerializationDelegate OnSerialization;

		public SerializationCallback(IServerChunk chunk)
		{
			_chunk = chunk;			
		}

		[ProtoBeforeSerialization]
		private void BeforeSerialization()
		{
			OnSerialization(_chunk);
		}
	}

	[ProtoContract]
	public class SnitchChunkData
	{
		[ProtoMember(1)]
		public Dictionary<BlockPos, Queue<SnitchViolation>> SnitchData;

		public SnitchChunkData(byte[] bytes)
		{
			SnitchData = Deserialize(bytes);
		}			
		public SnitchChunkData()
		{

		}

		public byte[] Serialize()
		{
			Dictionary<BlockPos, List<SnitchViolation>> convertedViolations = new Dictionary<BlockPos, List<SnitchViolation>>();
			foreach(var kvp in SnitchData)
			{
				convertedViolations.Add(kvp.Key, kvp.Value.ToList());
			}
			
			return SerializerUtil.Serialize<Dictionary<BlockPos, List<SnitchViolation>>>(convertedViolations);
		}	

		public Dictionary<BlockPos, Queue<SnitchViolation>> Deserialize(byte[] data)
		{
			Dictionary<BlockPos, Queue<SnitchViolation>> violations = new Dictionary<BlockPos, Queue<SnitchViolation>>();
			Dictionary<BlockPos, List<SnitchViolation>> convertedViolations = data == null ? null : SerializerUtil.Deserialize<Dictionary<BlockPos, List<SnitchViolation>>>(data);
			if (convertedViolations == null) return null;

			foreach(var kvp in convertedViolations)
			{				
				violations.Add(kvp.Key, new Queue<SnitchViolation>(kvp.Value));
			}		
			SnitchData = violations;

			return violations;
		}
	}

	[ProtoContract]
	public class ViolationData
	{
		[ProtoMember(1)]
		public List<SnitchViolation> data;

		ViolationData(Queue<SnitchViolation> violationData)
		{
			data = violationData.ToList<SnitchViolation>();
		}
	}

	public class ViolationLogger
	{
		private BlockEntitySnitch beSnitch;
		private readonly ConditionalWeakTable<IServerChunk, SnitchChunkData> _SnitchChunks = new();
		ICoreServerAPI sapi;

		public ViolationLogger(BlockEntitySnitch beSnitch, ICoreServerAPI sapi)
		{
			this.beSnitch = beSnitch;			
			this.sapi = sapi;
		}		

		public Queue<SnitchViolation> GetViolations(int count)
		{
			IServerChunk chunk = sapi.WorldManager.GetChunk(beSnitch.Pos);
			Queue<SnitchViolation> tempViolations;
			
			if (!_SnitchChunks.TryGetValue(chunk, out SnitchChunkData chunkData))
			{
				chunkData = AddChunkToDictionary(chunk, false);
			}
						
			chunkData.SnitchData.TryGetValue(beSnitch.Pos, out Queue<SnitchViolation> violations);
			if (violations == null) { return new Queue<SnitchViolation>(); }
			
			tempViolations = new Queue<SnitchViolation>();
			int tempCount = violations.Count;
			for(int i = 0; (i < count && i < tempCount); i++)
			{
				tempViolations.Enqueue(violations.Dequeue());
			}

			beSnitch.violationCount = violations.Count;

			chunk.MarkModified();

			return tempViolations;					
					
		}

		public void AddViolation(SnitchViolation violation)
		{
			IServerChunk chunk = sapi.WorldManager.GetChunk(beSnitch.Pos);
			
			if(!_SnitchChunks.TryGetValue(chunk, out SnitchChunkData chunkData)) { 
				chunkData = AddChunkToDictionary(chunk, true);
				if (chunkData.SnitchData == null)
				{
					chunkData.SnitchData = new Dictionary<BlockPos, Queue<SnitchViolation>>();
				}
			}
			
			if(!chunkData.SnitchData.TryGetValue(beSnitch.Pos, out Queue<SnitchViolation> violations))
			{
				if(violations == null) { violations = new(); }				
			}						

			violations.Enqueue(violation);
			chunkData.SnitchData[beSnitch.Pos] = violations;
			beSnitch.violationCount = violations.Count;

			chunk.MarkModified();
			
		}

		
		private SnitchChunkData AddChunkToDictionary(IServerChunk chunk, bool create) {
			
			byte[] data = chunk.GetServerModdata("Snitches");
			SnitchChunkData chunkData = new SnitchChunkData(data);
			if (chunkData.SnitchData == null)
			{
				if (!create)
				{
					return null;
				}
				else
				{
					chunkData = new();
				}
			} 

			_SnitchChunks.Add(chunk, chunkData);
			if (!chunk.LiveModData.TryGetValue("Snitches", out object serializerObj) || serializerObj is not SerializationCallback serializer) {
				serializer = new(chunk);
				chunk.LiveModData["Snitches"] = serializer;
			}
			serializer.OnSerialization += chunk => { 
				chunk.SetServerModdata("Snitches", chunkData.Serialize());
			};
			return chunkData;
		}

	}
}
