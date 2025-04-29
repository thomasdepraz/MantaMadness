using Unity.Mathematics;

public class ControllerStats
{
    SimpleController controller;
    ControllerData data;

    public ControllerStats(SimpleController controller, ControllerData data)
    {
        this.controller = controller;
        this.data = data;
    }

    public float GetGrip()
    {
        return controller.IsDrifting ? data.driftingGrip : data.grip;
    }

    //turn in -1/1 range
    public float GetSteering(float turn, bool drifting = false, int driftDir = 0)
    {
        if (drifting == false)
        {
            return turn * data.baseTurnSpeed;
        }

        //remap
        float minSteer = driftDir == 1 ? 1 : data.steeringMult;
        float maxSteer = driftDir == 1 ? data.steeringMult : 1;
        float remappedTurn = math.remap(-1, 1, minSteer, maxSteer, turn);

        return driftDir * remappedTurn * data.baseTurnSpeed;
    }

}
