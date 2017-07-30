﻿using Redzen.Numerics;
using SharpNeat.Neat.Genome;
using SharpNeat.Utils;
using System;
using System.Collections.Generic;

namespace SharpNeat.Neat
{
    public class NeatPopulationFactory<T> where T : struct
    {
        #region Instance Fields

        readonly MetaNeatGenome _metaNeatGenome;
        readonly double _connectionsProportion;

        readonly ConnectionDefinition[] _connectionDefArr;
        
        readonly IRandomSource _rng;
        readonly UInt32Sequence _genomeIdSeq;
        readonly UInt32Sequence _innovationIdSeq;
        readonly Func<T> _rngWeightFn;

        #endregion

        #region Constructor

        private NeatPopulationFactory(MetaNeatGenome metaNeatGenome, double connectionsProportion)
        {
            _metaNeatGenome = metaNeatGenome;
            _connectionsProportion = connectionsProportion;

            // Define the set of all possible connections between the input and output nodes (fully interconnected).
            int inCount = metaNeatGenome.InputNodeCount;
            int outCount = metaNeatGenome.OutputNodeCount;
            _connectionDefArr = new ConnectionDefinition[inCount * outCount];

            // Notes.
            // Connections and nodes are assigned innovation IDs from the same ID space (from the same 'pool' of numbers).
            // By convention the input nodes are assigned IDs first starting at zero, then the output nodes. Thus, because all 
            // of the evolved networks have a fixed number of inputs and outputs, the IDs of these nodes are fixed by convention.
            // Here we also allocate ID to connections, and these start at the first ID after the last output node. From there evolution
            // will create connections and nodes, and IDs are allocated in whatever order the nodes and connections are created in.
            int firstOutputNodeId = inCount;
            uint nextConnectionId = (uint)(inCount + outCount);

            for(int srcId=0, i=0; srcId < inCount; srcId++) {
                for(int tgtIdx=0; tgtIdx < outCount; tgtIdx++) {
                    _connectionDefArr[i++] = new ConnectionDefinition(nextConnectionId++, srcId, firstOutputNodeId + tgtIdx);
                }
            }

            _rng = RandomFactory.Create();
            _genomeIdSeq = new UInt32Sequence();
            _innovationIdSeq = new UInt32Sequence(nextConnectionId);

            // Init random connection weight source.
            if(typeof(T) == typeof(double)) {
                _rngWeightFn = (Func<T>)(object)RandomWeightSourceFactory.Create_Double(_metaNeatGenome.ConnectionWeightRange, _rng);
            }
            else if(typeof(T) == typeof(double)) {
                _rngWeightFn = (Func<T>)(object)RandomWeightSourceFactory.Create_Single((float)_metaNeatGenome.ConnectionWeightRange, _rng);
            }
            else {
                throw new ArgumentException("Unsupported type argument");
            }
        }

        #endregion

        #region Private Methods

        public NeatPopulation<T> CreatePopulation(int size)
        {
            var genomeList = CreateGenomeList(size);
            return new NeatPopulation<T>(_genomeIdSeq, _innovationIdSeq, genomeList, _metaNeatGenome);
        }

        /// <summary>
        /// Creates a list of randomly initialised genomes.
        /// </summary>
        /// <param name="length">The number of genomes to create.</param>
        private List<NeatGenome<T>> CreateGenomeList(int count)
        {
            List<NeatGenome<T>> genomeList = new List<NeatGenome<T>>(count);
            for(int i=0; i < count; i++) 
            {
                NeatGenome<T> genome = CreateGenome();
                genomeList.Add(genome);
            }
            return genomeList;
        }

        /// <summary>
        /// Creates a single randomly initialised genome.
        /// </summary>
        private NeatGenome<T> CreateGenome()
        {
            // Determine how many connections to create in the new genome, as a proportion of all possible connections
            // between the input and output nodes.
            int requiredConnectionCount = (int)NumericsUtils.ProbabilisticRound(_connectionDefArr.Length * _connectionsProportion, _rng);

            // Ensure there is at least one connection.
            requiredConnectionCount = Math.Max(1, requiredConnectionCount);

            // Select a random subset of all possible connections between the input and output nodes.
            int totalConnectionCount = _connectionDefArr.Length;
            int[] sampleArr = new int[requiredConnectionCount];
            DiscreteDistributionUtils.SampleUniformWithoutReplacement(totalConnectionCount, sampleArr, _rng);

            // Sort the samples.
            // Note. This helps keep the neural net connections (and thus memory accesses) in-order.
            Array.Sort(sampleArr);

            // Create the connection gene list and populate it.
            var connectionGeneArr = new ConnectionGene<T>[requiredConnectionCount];

            for(int i=0; i < sampleArr.Length; i++)
            {
                ConnectionDefinition cdef = _connectionDefArr[sampleArr[i]];

                ConnectionGene<T> cgene = new ConnectionGene<T>(
                    cdef._connectionId,
                    cdef._srcNodeId,
                    cdef._tgtNodeId,
                    _rngWeightFn());

                connectionGeneArr[i] = cgene;
            }

            // Get create a new genome with a new ID, birth generation of zero.
            uint id = _genomeIdSeq.Next();
            return new NeatGenome<T>(_metaNeatGenome, id, 0, connectionGeneArr);
        }

        #endregion

        #region Public Static Factory Method

        /// <summary>
        /// Create a new NeatPopulation with randomly initialised genomes.
        /// Genomes are randomly initialised by giving each a random subset of all possible connections between the input and output layer.
        /// </summary>
        /// <param name="metaNeatGenome">Genome metadata, e.g. the number of input and output nodes that each genome should have.</param>
        /// <param name="connectionsProportion">The proportion of possible connections between the input and output layers, to create in each new genome.</param>
        /// <param name="popSize">Population size. The number of new genomes to create.</param>
        /// <returns>A new NeatPopulation.</returns>
        public static NeatPopulation<T> CreatePopulation(MetaNeatGenome metaNeatGenome, double connectionsProportion, int popSize)
        {
            var factory = new NeatPopulationFactory<T>(metaNeatGenome, connectionsProportion);
            return factory.CreatePopulation(popSize);
        }

        #endregion

        #region Inner Class [ConnectionDefinition]

        struct ConnectionDefinition
        {
            public readonly uint _connectionId;
            public readonly int _srcNodeId;
            public readonly int _tgtNodeId;

            public ConnectionDefinition(uint innovationId, int srcNodeId, int tgtNodeId)
            {
                _connectionId = innovationId;
                _srcNodeId = srcNodeId;
                _tgtNodeId = tgtNodeId;
            }
        }

        #endregion
    }
}
