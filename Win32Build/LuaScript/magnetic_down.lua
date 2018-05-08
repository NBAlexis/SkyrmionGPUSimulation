
-- Initial the magnetic by point down
function GetMagneticByLatticeIndex(x, y)
    return 0, 0, -1
end

-- Need to register the function
return { 
    GetMagneticByLatticeIndex = GetMagneticByLatticeIndex,
}
