
-- Initial the magnetic by point down
function GetMagneticByLatticeIndex(x, y)
    if (x - 255) * (x - 255) + (y - 255) * (y - 255) < 100 then
        return 0, 0, -1
    end
    return 0, 0, 1
end
