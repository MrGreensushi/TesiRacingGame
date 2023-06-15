using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Offline_Dispatcher : MonoBehaviour
{
    [SerializeField] int timesteps, featuresNumber;
    [SerializeField] Offline_GhostCar car;
    [SerializeField] OfflineCar carTrue;
    [SerializeField] Offline_Physic_Engine pe;
    [SerializeField] Offline_Rule_Engine re;
    private bool updateSelf = false;

    public bool UpdateSelf { set { updateSelf = value; } }


    public void Routine()
    {

        var (delta, infos) = PhysicInfos();
        var (delta_T, infos_T) = TruePhysicInfos();
        car.lastInfo = infos;
        carTrue.lastInfo = infos_T;

        if (updateSelf)
            pe.UpdateMatrix(delta, timesteps, featuresNumber, infos);

        else
            pe.UpdateMatrix(delta_T, timesteps, featuresNumber, infos_T);




    }

    (float[], float[]) PhysicInfos()
    {
        return car.PhysicInfos();

    }

    (float[], float[]) TruePhysicInfos()
    {
        return carTrue.DispatcherInfos();

    }

    float[] RuleInfos()
    {
        return new float[0];
    }

}
