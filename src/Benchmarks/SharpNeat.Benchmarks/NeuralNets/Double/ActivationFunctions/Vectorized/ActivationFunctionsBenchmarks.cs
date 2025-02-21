﻿using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using Redzen.Numerics.Distributions.Double;

namespace SharpNeat.NeuralNets.Double.ActivationFunctions.Vectorized.Benchmarks;

/// <summary>
/// Double precision vectorized activation function benchmarks.
/// </summary>
public class ActivationFunctionsBenchmarks
{
    #region Static Fields


    static readonly IActivationFunction<double> __LeakyReLU = new LeakyReLU();
    static readonly IActivationFunction<double> __LeakyReLUShifted = new LeakyReLUShifted();
    static readonly IActivationFunction<double> __MaxMinusOne = new MaxMinusOne();
    static readonly IActivationFunction<double> __PolynomialApproximantSteep = new PolynomialApproximantSteep();
    static readonly IActivationFunction<double> __QuadraticSigmoid = new QuadraticSigmoid();
    static readonly IActivationFunction<double> __ReLU = new ReLU();
    static readonly IActivationFunction<double> __SoftSignSteep = new SoftSignSteep();
    static readonly IActivationFunction<double> __SReLU = new SReLU();
    static readonly IActivationFunction<double> __SReLUShifted = new SReLUShifted();

    #endregion

    #region Instance Fields

    const int __loops = 1000;
    readonly double[] _x = new double[1003];
    readonly double[] _w = new double[1003];

    #endregion

    #region Constructor

    public ActivationFunctionsBenchmarks()
    {
        // Create some random Gaussian values as the inputs to the activation functions.
        var gaussian = new ZigguratGaussianSampler(0.0, 2.0, 0);
        for(int i=0; i < _x.Length; i++)
            _x[i] = gaussian.Sample();
    }

    #endregion

    #region Public Methods

    [Benchmark]
    public void LeakyReLU()
    {
        RunBenchmark(__LeakyReLU);
    }

    [Benchmark]
    public void LeakyReLUShifted()
    {
        RunBenchmark(__LeakyReLUShifted);
    }


    [Benchmark]
    public void MaxMinusOne()
    {
        RunBenchmark(__MaxMinusOne);
    }

    //[Benchmark]
    //public void NullFn()
    //{
    //    RunBenchmark(__NullFn);
    //}

    [Benchmark]
    public void PolynomialApproximantSteep()
    {
        RunBenchmark(__PolynomialApproximantSteep);
    }

    [Benchmark]
    public void QuadraticSigmoid()
    {
        RunBenchmark(__QuadraticSigmoid);
    }

    [Benchmark]
    public void ReLU()
    {
        RunBenchmark(__ReLU);
    }


    [Benchmark]
    public void SoftSignSteep()
    {
        RunBenchmark(__SoftSignSteep);
    }

    [Benchmark]
    public void SReLU()
    {
        RunBenchmark(__SReLU);
    }

    [Benchmark]
    public void SReLUShifted()
    {
        RunBenchmark(__SReLUShifted);
    }


    #endregion

    #region Private Methods

    private void RunBenchmark(IActivationFunction<double> actFn)
    {
        ref double xref = ref MemoryMarshal.GetReference(_x.AsSpan());
        ref double wref = ref MemoryMarshal.GetReference(_w.AsSpan());
        int len = _x.Length;

        for(int i=0; i < __loops; i++)
            actFn.Fn(ref xref, ref wref, len);
    }

    #endregion
}
