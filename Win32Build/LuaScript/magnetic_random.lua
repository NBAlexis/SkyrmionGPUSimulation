
-- Initial the magnetic by random
function GetMagneticByLatticeIndex(x, y)
    local nx = math.random() * 2.0 - 1.0
    local ny = math.random() * 2.0 - 1.0
    local nz = math.random() * 2.0 - 1.0
    local length_inv = 1.0 / math.sqrt(nx * nx + ny * ny + nz * nz)
    length_inv = math.max(length_inv, 0.000000001)
    return nx * length_inv, ny * length_inv, nz * length_inv
end

-- Need to register the function
return { 
    GetMagneticByLatticeIndex = GetMagneticByLatticeIndex,
}
