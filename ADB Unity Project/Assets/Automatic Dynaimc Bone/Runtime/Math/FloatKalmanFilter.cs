using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Mathematics.Extensions;
using System;
using Random = Unity.Mathematics.Random;
using System.Runtime.CompilerServices;

public struct FloatKalmanFilter
{
    /// <summary>
    /// Process noise matrix
    /// </summary>
    public float3x3 processNoiseCovMat;

    /// <summary>
    ///  measurement noise covience matrix
    /// </summary>
    private float3x3 measurementNoiseCovMat;

    /// <summary>
    /// fliter noise covariance  Mat
    /// </summary>
    private float3x3 fliterNoiseCovMat;

    /// <summary>
    ///  current State matrix
    /// </summary>
    private float3x3 transitionMat;

    /// <summary>
    ///  current State matrix
    /// </summary>
    private float3 currentState;

    private float oldPosition;

    private float oldDeltaPosition;

    public FloatKalmanFilter(float position, float deltaPosition, float processCovariance = 1, float positionDeviation = 3, float deltaPositionDeviation = 7, float deltaDeltaPositionDeviation = 12)
    {
        oldPosition = position;
        oldDeltaPosition = deltaPosition;
        measurementNoiseCovMat = float3x3.identity;
        processNoiseCovMat = float3x3.identity;
        fliterNoiseCovMat = float3x3.identity;

        currentState = new float3(position, deltaPosition, 0);
        measurementNoiseCovMat = 0;

        transitionMat = float3x3.identity;

        SetParameter(processCovariance, positionDeviation, deltaPositionDeviation, deltaDeltaPositionDeviation);
    }
    public void SetParameter(float processCovariance, float positionDeviation, float deltaPositionDeviation, float deltaDeltaPositionDeviation)
    {
        float cP = math.exp(positionDeviation);
        float cV = math.exp(deltaPositionDeviation);
        float cA = math.exp(deltaDeltaPositionDeviation);

        measurementNoiseCovMat = new float3x3(
            cP, 0, 0,
            0, cV, 0,
            0, 0, cA);
        processNoiseCovMat = float3x3.identity * processCovariance;
    }

    public float Update(float position, float deltaTime)
    {
        if (float.IsNaN(position))
        {
            throw new InvalidOperationException("Kalman input data cannot be NaN");
        }
        float dt = deltaTime;
        if (dt > 0)
        {
            float dt2 = dt * dt * 0.5f;

            transitionMat = new float3x3( //L=vt+0.5f*at^2
                1,dt,dt2,
                0,1,dt,
                0,0,1);

            KalmanPredict(transitionMat, processNoiseCovMat);

            float deltaPosition = (position - oldPosition) / dt;
            float deltaDeltaPosition = (deltaPosition - oldDeltaPosition) / dt;
            float3 measurement = new float3(position, deltaPosition, deltaDeltaPosition);
            KalmanUpdate(measurement, float3x3.identity, measurementNoiseCovMat);

            oldPosition = position;
            oldDeltaPosition = deltaPosition;
        }
        return currentState.x;
    }

    private void KalmanPredict(float3x3 transitionMat, float3x3 processNoiseMat)
    {
        currentState = mulT(transitionMat, currentState);

        float3x3 P = mul(fliterNoiseCovMat, transitionMat);
        fliterNoiseCovMat = mulT(transitionMat, P) + processNoiseMat;
    }

    private void KalmanUpdate(float3 measure, float3x3 measurementMat, float3x3 measurementNoiseCovMat)
    {
        float3x3 K1 = mul(fliterNoiseCovMat, measurementMat);
        float3x3 K2 = mulT(measurementMat, K1) + measurementNoiseCovMat;
        float3x3 K = mulT(K1, math.inverse(K2));

        float3 loss = measure - (mulT(measurementMat, currentState));
        float3 loss1 = mulT(K, loss);

        currentState = currentState + loss1;

        float3x3 identity = float3x3.identity;
        fliterNoiseCovMat = mulT(identity - mulT(K, measurementMat), fliterNoiseCovMat);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float mul(float3 lhs, float3 rhs)
    {
        return csum(lhs * rhs);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float3 mul(float3 lhs, float3x3 rhs)
    {
        float3 result = 0;
        for (int i = 0; i < 3; i++)
        {
            result[i] = mul(lhs, rhs[i]);
        }
        return result;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float3 mulT(float3x3 lhs, float3 rhs)
    {
        return mul(rhs, lhs);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float3x3 mulT(float3x3 lhs, float3x3 rhs)
    {
        float3x3 result = 0;
        rhs = math.transpose(rhs);
        for (int i = 0; i < 3; i++)
        {
            result[i] = mul(lhs[i], rhs);
        }
        return result;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float3x3 mul(float3x3 lhs, float3x3 rhs)
    {
        float3x3 result = 0;
        for (int i = 0; i < 3; i++)
        {
            float3 temp = mul(lhs[i], rhs);
            result[i] = temp;
        }
        return result;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float csum(float3 lhs)
    {
        return math.csum(lhs);
    }


}
