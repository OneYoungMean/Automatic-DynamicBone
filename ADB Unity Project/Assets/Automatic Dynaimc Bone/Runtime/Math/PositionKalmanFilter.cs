using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Mathematics.Extensions;
using System;
using Random = Unity.Mathematics.Random;
using System.Runtime.CompilerServices;

public struct PositionKalmanFilter
{
    /// <summary>
    /// Process noise matrix
    /// </summary>
    public float9x9 processNoiseCovMat;

    /// <summary>
    ///  measurement noise covience matrix
    /// </summary>
    private float9x9 measurementNoiseCovMat;

    /// <summary>
    /// fliter noise covariance  Mat
    /// </summary>
    private float9x9 fliterNoiseCovMat;

    /// <summary>
    ///  current State matrix
    /// </summary>
    private float9x9 transitionMat;

    /// <summary>
    ///  current State matrix
    /// </summary>
    private float9 currentState;

    private float3 oldPosition;

    private float3 oldDeltaPosition;

    public PositionKalmanFilter(float3 position, float3 deltaPosition, float processCovariance=1, float positionDeviation=3, float deltaPositionDeviation=7, float deltaDeltaPositionDeviation=12)
    {
        oldPosition = position;
        oldDeltaPosition= deltaPosition; 
        measurementNoiseCovMat= float9x9.identity;
        processNoiseCovMat = float9x9.identity;
        fliterNoiseCovMat = float9x9.identity;

         currentState = new float9(position, deltaPosition, 0);
        measurementNoiseCovMat = 0;

        transitionMat = new float9x9(
            1, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 1, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 1, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 1, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 1, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 1, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 1, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 1, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 1
            );

        SetParameter(processCovariance, positionDeviation, deltaPositionDeviation, deltaDeltaPositionDeviation);
    }
    public void SetParameter(float processCovariance, float positionDeviation, float deltaPositionDeviation, float deltaDeltaPositionDeviation)
    {
        float cP =math.exp(positionDeviation);
        float cV = math.exp(deltaPositionDeviation );
        float cA = math.exp(deltaDeltaPositionDeviation );

        measurementNoiseCovMat = new float9x9(
            cP, 0, 0, 0, 0, 0, 0, 0, 0,
            0, cP, 0, 0, 0, 0, 0, 0, 0,
            0, 0, cP, 0, 0, 0, 0, 0, 0,
            0, 0, 0, cV, 0, 0, 0, 0, 0,
            0, 0, 0, 0, cV, 0, 0, 0, 0,
            0, 0, 0, 0, 0, cV, 0, 0, 0,
            0, 0, 0, 0, 0, 0, cA, 0, 0,
            0, 0, 0, 0, 0, 0, 0, cA, 0,
            0, 0, 0, 0, 0, 0, 0, 0, cA
            );
        processNoiseCovMat = float9x9.identity * processCovariance;
    }

    public float3 Update(float3 position, float deltaTime)
    {
        if (float.IsNaN(position.x)||float.IsNaN(position.y)||float.IsNaN(position.z))
        {
            throw new InvalidOperationException("Kalman input data cannot be NaN");
        }
        float dt = deltaTime;
        if (dt > 0)
        {
            float dt2 = dt * dt*0.5f;

            transitionMat = new float9x9( //L=vt+0.5f*at^2
               new float9(1, 0, 0, dt, 0, 0, dt2, 0, 0),
               new float9(0, 1, 0, 0, dt, 0, 0, dt2, 0),
               new float9(0, 0, 1, 0, 0, dt, 0, 0, dt2),
               new float9(0, 0, 0, 1, 0, 0, dt, 0, 0),
               new float9(0, 0, 0, 0, 1, 0, 0, dt, 0),
               new float9(0, 0, 0, 0, 0, 1, 0, 0, dt),
               new float9(0, 0, 0, 0, 0, 0, 1, 0, 0),
               new float9(0, 0, 0, 0, 0, 0, 0, 1, 0),
               new float9(0, 0, 0, 0, 0, 0, 0, 0, 1)
                );

            KalmanPredict(transitionMat, processNoiseCovMat);

            float3 deltaPosition = (position - oldPosition) / dt;
            float3 deltaDeltaPosition = (deltaPosition - oldDeltaPosition) / dt;
            float9 measurement = new float9(position, deltaPosition, deltaDeltaPosition);
            KalmanUpdate(measurement, float9x9.identity, measurementNoiseCovMat);

            oldPosition = position;
            oldDeltaPosition = deltaPosition;
        }
        return new float3(currentState.r0, currentState.r1, currentState.r2);
    }

    private void KalmanPredict(float9x9 transitionMat, float9x9 processNoiseMat)
    {
        currentState = mulT(transitionMat, currentState);

        float9x9 P = mul(fliterNoiseCovMat, transitionMat); 
        fliterNoiseCovMat = mulT(transitionMat ,P)+ processNoiseMat;
    }

    private void KalmanUpdate(float9 measure, float9x9 measurementMat, float9x9 measurementNoiseCovMat)
    {
        float9x9 K1 =mul(fliterNoiseCovMat, measurementMat);
        float9x9 K2 = mulT(measurementMat, K1) + measurementNoiseCovMat;
        float9x9 K = mulT(K1, K2.inverse());

        float9 loss = measure - (mulT(measurementMat, currentState));
        float9 loss1 = mulT(K, loss);

        currentState = currentState + loss1;

        float9x9 identity = float9x9.identity;
        fliterNoiseCovMat = mulT(identity - mulT(K, measurementMat), fliterNoiseCovMat);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float mul(float9 lhs, float9 rhs)
    {
        return csum(lhs * rhs);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float9 mul(float9 lhs, float9x9 rhs)
    {
        float9 result = 0;
        for (int i = 0; i < 9; i++)
        {
            result[i] = mul(lhs, rhs[i]);
        }
        return result;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float9 mulT( float9x9 lhs, float9 rhs)
    {
        return mul(rhs, lhs); 
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float9x9 mulT(float9x9 lhs, float9x9 rhs)
    {
        float9x9 result = 0;
        rhs= rhs.transpose();
        for (int i = 0; i < 9; i++)
        {
            result[i] = mul(lhs[i], rhs);
        }
        return result;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float9x9 mul(float9x9 lhs, float9x9 rhs)
    {
        float9x9 result = 0;
        for (int i = 0; i < 9; i++)
        {
           float9 temp = mul(lhs[i], rhs);
            result[i]=temp;
        }
        return result;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float csum(float9 lhs)
    {
        return lhs.r0 + lhs.r1 + lhs.r2 + lhs.r3 + lhs.r4 + lhs.r5 + lhs.r6 + lhs.r7 + lhs.r8;
    }


}
