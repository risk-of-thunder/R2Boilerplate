using EntityStates.Loader;

namespace ToggledChargedGauntlet
{
    class ToggledChargeFist : BaseChargeFist
    {
        bool keyReleased { get; set; } = false;
         public override bool ShouldKeepChargingAuthority()
        {
            return  !base.IsKeyDownAuthority() || !keyReleased;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!base.IsKeyDownAuthority())
            {
                keyReleased = true;
            }
        }
    }
}
