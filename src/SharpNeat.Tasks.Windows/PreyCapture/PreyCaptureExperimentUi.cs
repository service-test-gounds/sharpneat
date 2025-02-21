﻿// This file is part of SharpNEAT; Copyright Colin D. Green.
// See LICENSE.txt for details.
using SharpNeat.Experiments;
using SharpNeat.Neat.Genome.Double;
using SharpNeat.Tasks.PreyCapture;
using SharpNeat.Tasks.PreyCapture.ConfigModels;
using SharpNeat.Windows;
using SharpNeat.Windows.Neat;

namespace SharpNeat.Tasks.Windows.PreyCapture;

/// <summary>
/// Implementation of <see cref="IExperimentUi"/> for the Prey Capture task.
/// </summary>
public sealed class PreyCaptureExperimentUi : NeatExperimentUi
{
    readonly INeatExperiment<double> _neatExperiment;
    readonly PreyCaptureCustomConfig _customConfig;

    public PreyCaptureExperimentUi(
        INeatExperiment<double> neatExperiment,
        PreyCaptureCustomConfig customConfig)
    {
        _neatExperiment = neatExperiment ?? throw new ArgumentNullException(nameof(neatExperiment));
        _customConfig = customConfig ?? throw new ArgumentNullException(nameof(customConfig));
    }

    /// <inheritdoc/>
    public override GenomeControl CreateTaskControl()
    {
        PreyCaptureWorld world = new(
            _customConfig.PreyInitMoves,
            _customConfig.PreySpeed,
            _customConfig.SensorRange,
            _customConfig.MaxTimesteps);

        var genomeDecoder = NeatGenomeDecoderFactory.CreateGenomeDecoder(
            _neatExperiment.IsAcyclic,
            _neatExperiment.EnableHardwareAcceleratedNeuralNets);

        return new PreyCaptureControl(genomeDecoder, world);
    }
}
