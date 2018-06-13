-- Exchange Strength pin at 3 points
function GetJValueByLatticeIndex(x, y)
    local j0 = 1
    local j1 = 1

    -- pin radus ~ 1 / sqrt(j2) = 18
    local j2 = 0.003

    local rho1 = (x - 150) * (x - 150) + (y - 127) * (y - 127)
    local rho2 = (x - 150) * (x - 150) + (y - 255) * (y - 255)
    local rho3 = (x - 150) * (x - 150) + (y - 383) * (y - 383)

    return j0 + j1 * (math.exp(-1.0 * j2 * rho1) + math.exp(-1.0 * j2 * rho2) + math.exp(-1.0 * j2 * rho3))
end
