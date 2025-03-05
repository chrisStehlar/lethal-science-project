using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

public partial class Limb : Enemy
{
    public override void Pacify()
    {
        GetParent<Enemy>().Pacify();
    }
}
