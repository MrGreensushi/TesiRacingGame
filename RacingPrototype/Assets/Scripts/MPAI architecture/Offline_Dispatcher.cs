using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Offline_Dispatcher : MonoBehaviour
{
    public int timesteps, featuresNumber;
    [SerializeField] Offline_GhostCar car;
    [SerializeField] OfflineCar carTrue;
    [SerializeField] Offline_Physic_Engine pe;
    public OperatingMode operatingMode;


    //DELTA VERSION
    public void Routine()
    {

        var (delta, infos) = PhysicInfos();
        var (delta_T, infos_T) = TruePhysicInfos();
        car.lastInfo = infos;
        carTrue.lastInfo = infos_T;

        if (operatingMode != OperatingMode.Testing)
            UpdateMatrix(delta, infos);

        else
            UpdateMatrix(delta_T, infos_T);




    }

    void UpdateMatrix(float[] d, float[] i)
    {

        pe.UpdateMatrix(d, timesteps, featuresNumber, i);
    }

    (float[], float[]) PhysicInfos()
    {
        return car.PhysicInfos();

    }

    (float[], float[]) TruePhysicInfos()
    {
        return carTrue.DispatcherInfos();

    }

    //REAL

    //public void Routine()
    //{

    //    var infos = PhysicInfos();
    //    var infos_t = TruePhysicInfos();

    //    if (updateSelf)
    //        pe.UpdateMatrix(infos, timesteps, featuresNumber);

    //    else
    //        pe.UpdateMatrix(infos_t, timesteps, featuresNumber);
    //}

    //float[] PhysicInfos()
    //{
    //    return car.PhysicInfos();

    //}

    //float[] TruePhysicInfos()
    //{
    //    return carTrue.DispatcherInfos();

    //}

    float[] RuleInfos()
    {
        return new float[0];
    }

}
