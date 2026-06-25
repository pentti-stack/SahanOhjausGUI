namespace SahanOhjausGUI
{
    public class TeraParametrit
    {
        public int TeraNumero { get; set; }
        public bool OnkoVasenKatinen { get; set; }
        public double Rako { get; set; }
        public double Runko { get; set; }
        public double Laippa { get; set; }

        public double LaskeOffset()
        {
            if (OnkoVasenKatinen)
                return Laippa - (Runko / 2.0);
            else
                return Runko / 2.0;
        }
    }
}