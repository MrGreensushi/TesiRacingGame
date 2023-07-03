using QuickStart;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickStart
{
    public class Dispatcher : MonoBehaviour
    {
        public int timesteps, featuresNumber;
        public Ghost_Car car;
        public PlayerScript carTrue;
        [SerializeField] Physic_Engine pe;
        public bool updateSelf = false;

        public bool UpdateSelf { set { updateSelf = value; } }

        //DELTA VERSION
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
    }
}
