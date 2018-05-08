-- Exchange Strength is pin
function GetJValueByLatticeIndex(x, y)
    local j0 = 1
    local j1 = 1

    -- pin radus ~ 1 / sqrt(j2), this is a about 30 in a 512 x 512 lattice
    local j2 = 0.001
    -- Center point is 255
    local rho = (x - 255) * (x - 255) + (y - 255) * (y - 255)

    return j0 + j1 * math.exp(-1.0 * j2 * rho)
end

-- Need to register the function
return { 
    GetJValueByLatticeIndex = GetJValueByLatticeIndex,
}
